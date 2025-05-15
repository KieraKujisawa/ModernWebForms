using System;
using System.Collections;
using System.Configuration;
using System.Web.Security.AntiXss;

namespace WebForms.Helper.Page;

public class ViewStateServerManager
{
    // This uses an array and mod to cycle repeatedly through an array (so limited size)

    //WARNING:  When the user uses the "back" button on the browser, IE will not rerequest the page, so 
    // if the user posts again they need the viewstate to still be present on the server.  Need to set the VIEW_STATE_NUM_PAGES
    // to a tradeoff of number of back pages allowed and 
    // the amount of memory consumed by the viewstate kept per page.  
    private const short VIEW_STATE_NUM_PAGES = 5;       //Number of pages to keep viewstate for

    //Name of storage location for viewstate information
    private const string SESSION_VIEW_STATE_MGR = "VIEW_STATE_MGR";

    private const string PAGE_INSTANCE_FIELD_NAME = "PAGE_INSTANCE_FIELD_NAME";

    private const string URL_INSTANCE_KEY = "instanceguid";

    private long lPageCount = 0;    //Number of pages seen by this customer 
    private string[] ViewStates = new string[VIEW_STATE_NUM_PAGES]; //Store for viewstates

    //Determine if server side is enabled or not from web.config file
    public bool ServerSideEnabled
    {
        get
        {
            //Not a problem if someone changes the value in web.config, because new AppDomain will
            // be started and all in process session is lost anyway
            return Convert.ToBoolean(ConfigurationManager.AppSettings["ServerSideViewState"]);
        }
    }

    public ViewStateServerManager()
    {
    }

    public long SaveViewState(string szViewState)
    {
        //Increment the total page seen counter
        lPageCount++;

        //Now use the modulas operator (%) to find remainder of that and size of viewstate storage, this creates a
        // circular array where it continually cycles through the array index range (effectively keeps
        // the last requests to match size of storage)
        short siIndex = (short)(lPageCount % VIEW_STATE_NUM_PAGES);

        //Now save the viewstate for this page to the current position.  
        ViewStates[siIndex] = szViewState;

        return lPageCount;
    }


    public string GetViewState(long lRequestNumber)
    {
        //Could cycle though the array and make sure that the given request number is actually
        // present (in case the array is not big enough).  Much faster to just take the
        // given request number and recalculate where it should be stored
        short siIndex = (short)(lRequestNumber % VIEW_STATE_NUM_PAGES);

        return ViewStates[siIndex];
    }

    public string GetPageKey(BasePage inPage)
    {
        string sResult = inPage.Request.Path;
        // Check if the client has form field that stores request key
        System.Web.UI.HtmlControls.HtmlInputHidden oInstanceKeyControl = (System.Web.UI.HtmlControls.HtmlInputHidden)inPage.FindControl(PAGE_INSTANCE_FIELD_NAME);
        if (oInstanceKeyControl == null)
        {
            oInstanceKeyControl = new System.Web.UI.HtmlControls.HtmlInputHidden();
            oInstanceKeyControl.ID = PAGE_INSTANCE_FIELD_NAME;
            string sValue = AntiXssEncoder.XmlEncode(inPage.Request[URL_INSTANCE_KEY]);
            if (sValue == null) sValue = "1"; // Default;
            oInstanceKeyControl.Value = sValue;
        }
        string sInstanceValue = oInstanceKeyControl.Value;
        if (sInstanceValue == "") sInstanceValue = "1"; // Default
        sResult += "__INSTANCE=" + sInstanceValue;
        return sResult;
    }

    public static ViewStateServerManager GetInstance()
    {
        ViewStateServerManager oResult;

        // Not already in session, create a new one and put in session
        if (System.Web.HttpContext.Current.Session[SESSION_VIEW_STATE_MGR] == null)
        {
            oResult = new ViewStateServerManager();
            System.Web.HttpContext.Current.Session[SESSION_VIEW_STATE_MGR] = oResult;
        }
        else
        {
            oResult = (ViewStateServerManager)System.Web.HttpContext.Current.Session[SESSION_VIEW_STATE_MGR];
        }
        return oResult;
    }
}
