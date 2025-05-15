using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using System.Text;
using System.IO;


namespace WebForms.Helper.Page;

public class BasePage : System.Web.UI.Page
{
    //Setup the name of the hidden field on the client for storing the viewstate key
    public const string VIEW_STATE_FIELD_NAME = "BASEPAGE_VIEWSTATE";

    //Setup a formatter for the viewstate information
    private LosFormatter? _formatter = null;

    public BasePage()
    {
    }

    //overriding method of Page class
    protected override object? LoadPageStateFromPersistenceMedium()
    {
        //If server side enabled use it, otherwise use original base class implementation
        if (true == ViewStateServerManager.GetViewStateSvrMgr().ServerSideEnabled)
        {
            return LoadViewState();
        }
        else
        {
            return base.LoadPageStateFromPersistenceMedium();
        }
    }

    //overriding method of Page class
    protected override void SavePageStateToPersistenceMedium(object viewState)
    {
        if (true == ViewStateServerManager.GetViewStateSvrMgr().ServerSideEnabled)
        {
            SaveViewState(viewState);
        }
        else
        {
            base.SavePageStateToPersistenceMedium(viewState);
        }
    }

    //implementation of method
    private object? LoadViewState()
    {
        if (_formatter == null)
        {
            _formatter = new LosFormatter();
        }

        //Check if the client has form field that stores request key
        if (null == Request.Form[VIEW_STATE_FIELD_NAME])
        {
            //Did not see form field for viewstate, return null to try to continue (could log event...)
            return null;
        }

        //Make sure it can be converted to request number (in case of corruption)
        long lRequestNumber = 0;
        try
        {
            lRequestNumber = Convert.ToInt64(Request.Form[VIEW_STATE_FIELD_NAME]);
        }
        catch
        {
            //Could not covert to request number, return null (could log event...)
            return null;
        }

        //Get the viewstate for this page
        string _viewState = ViewStateServerManager.GetViewStateSvrMgr().GetViewState(lRequestNumber);

        //If find the viewstate on the server, convert it so ASP.Net can use it
        if (_viewState == null)
            return null;
        else
            return _formatter.Deserialize(_viewState);
    }


    //implementation of method
    private void SaveViewState(object viewState)
    {
        if (_formatter == null)
        {
            _formatter = new LosFormatter();
        }

        // parse the viewState
        StringWriter writer = new StringWriter();
        ViewStateParser p = new ViewStateParser(writer);

        p.ParseViewStateGraph(viewState);
        var state = writer.ToString();

        //Save the viewstate information
        StringBuilder _viewState = new StringBuilder();
        StringWriter _writer = new StringWriter(_viewState);
        _formatter.Serialize(_writer, viewState);
        long lRequestNumber = ViewStateServerManager.GetViewStateSvrMgr().SaveViewState(_viewState.ToString());

        //Need to register the viewstate hidden field (must be present or postback things don't 
        // work, we use in our case to store the request number)
        ClientScript.RegisterHiddenField(VIEW_STATE_FIELD_NAME, lRequestNumber.ToString());
    }
}
