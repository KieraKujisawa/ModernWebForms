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
using System.Collections.Generic;
using System.Linq;
using WebForms.Helper.Extensions;

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
        var inst = ViewStateServerManager.GetInstance();

        //If server side enabled use it, otherwise use original base class implementation
        if (true == inst.ServerSideEnabled)
        {
            LoadAddedControlStates(null);

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
        var inst = ViewStateServerManager.GetInstance();

        if (true == inst.ServerSideEnabled)
        {
            SaveViewState(viewState);
            SaveAddedControlStates(null);
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

        var inst = ViewStateServerManager.GetInstance();

        //Get the viewstate for this page
        string _viewState = inst.GetViewState(lRequestNumber);

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

        var inst = ViewStateServerManager.GetInstance();

        long lRequestNumber = inst.SaveViewState(_viewState.ToString());

        //Need to register the viewstate hidden field (must be present or postback things don't 
        // work, we use in our case to store the request number)
        ClientScript.RegisterHiddenField(VIEW_STATE_FIELD_NAME, lRequestNumber.ToString());
    }

    private void LoadAddedControlStates(System.Web.UI.Control? inControl)
    {
        System.Web.UI.ControlCollection oControlList = (inControl == null) ? Controls : inControl.Controls;
        for (int i = 0; i < oControlList.Count; i++)
        {
            if (Request.Params[oControlList[i].ID] != null) continue;

            LoadAddedControlStates(oControlList[i]);

            ViewStateServerManager sm = ViewStateServerManager.GetInstance();

            object? oValue = null;

            if (sm != null)
            {
                oValue = Session[sm.GetPageKey(this) + "__CONTROL: " + oControlList[i].ID];
            }

            if (oValue == null) continue;

            if (oControlList[i] is TextBox)
            {
                ((TextBox)oControlList[i]).Text = (string)oValue;
            }
            else if (oControlList[i] is RadioButton)
            {
                RadioButton oRB = (RadioButton)oControlList[i];
                string sGroupName = oRB.GroupName;
                bool bChecked = (bool)oValue;
                if (Request.Params[sGroupName] != null)
                {
                    string sRequestValue = (string)Request.Params[sGroupName];
                    bChecked = (sRequestValue == oControlList[i].ID);
                }
                ((System.Web.UI.WebControls.RadioButton)oControlList[i]).Checked = bChecked;
            }
            else if (oControlList[i] is CheckBox) ((CheckBox)oControlList[i]).Checked = (bool)oValue;
            else if (oControlList[i] is DropDownList) ((DropDownList)oControlList[i]).SelectedIndex = (Int32)oValue;

            else if (oControlList[i] is CheckBoxList) LoadListState(oControlList[i], oValue);
            else if (oControlList[i] is ListBox) LoadListState(oControlList[i], oValue);

            else if (oControlList[i] is HtmlInputText) ((HtmlInputText)oControlList[i]).Value = (string)oValue;
            else if (oControlList[i] is HtmlInputRadioButton)
            {
                HtmlInputRadioButton oRB = (HtmlInputRadioButton)oControlList[i];
                String sGroupName = oRB.Name;
                bool bChecked = (bool)oValue;
                if (Request.Params[sGroupName] != null)
                {
                    string sRequestValue = (string)Request.Params[sGroupName];
                    bChecked = (sRequestValue == oControlList[i].ID);
                }
                ((HtmlInputRadioButton)oControlList[i]).Checked = bChecked;
            }
            else if (oControlList[i] is HtmlInputCheckBox) ((HtmlInputCheckBox)oControlList[i]).Checked = (bool)oValue;
            else if (oControlList[i] is HtmlSelect) ((HtmlSelect)oControlList[i]).SelectedIndex = (Int32)oValue;
        }
    }

    protected void SaveAddedControlStates(System.Web.UI.Control? inControl)
    {
        System.Web.UI.ControlCollection oControlList = (inControl == null) ? Controls : inControl.Controls;
        for (int i = 0; i < oControlList.Count; i++)
        {
            SaveAddedControlStates(oControlList[i]);
            object? oValue = null;
            if (oControlList[i] is RadioButton) oValue = ((RadioButton)oControlList[i]).Checked;
            else if (oControlList[i] is TextBox) oValue = ((TextBox)oControlList[i]).Text;
            else if (oControlList[i] is CheckBox) oValue = ((CheckBox)oControlList[i]).Checked;
            else if (oControlList[i] is CheckBoxList) oValue = SaveListState(oControlList[i]);
            else if (oControlList[i] is DropDownList) oValue = ((DropDownList)oControlList[i]).SelectedIndex;
            else if (oControlList[i] is ListBox) oValue = SaveListState(oControlList[i]);
            else continue;

            ViewStateServerManager sm = ViewStateServerManager.GetInstance();

            if (sm != null)
            {
                Session[sm.GetPageKey(this) + "__CONTROL: " + oControlList[i].ID] = oValue;
            }

        }
    }

    private void LoadListState(Control inControl, object value)
    {
        var items = new List<string>();

        if (value.GetType().IsArrayOf<string>())
        {
            items = ((string[])value).ToList();
        }

        var lc = (ListControl)inControl;

        foreach (ListItem li in lc.Items)
        {
            foreach (string val in items)
            {
                if (li.Value == val)
                {
                    li.Selected = true;
                    break;
                }
            }
        }
    }

    protected string[] SaveListState(Control inControl)
    {
        var ret = new List<string>();

        var lc = (ListControl)inControl;

        foreach (ListItem li in lc.Items)
        {
            if (li.Selected == true)
                ret.Add(li.Value);
        }

        return ret.ToArray();
    }

    protected string[]? GetControlNames()
    {
        LoadControls(null, true);
        ArrayList oList = new ArrayList();
        foreach (DictionaryEntry itEntry in _lControls)
        {
            string sName = (string)itEntry.Key;
            oList.Add(sName);
        }
        oList.Add("PAGE");
        if (oList.Count <= 0) return null;
        string[] sResult = new string[oList.Count];
        for (int i = 0; i < oList.Count; i++) sResult[i] = (string)oList[i];
        return sResult;
    }

    private Hashtable _lControls = new Hashtable();
    private void LoadControls(Control? inControl, bool inReset)
    {
        if (inReset) _lControls = new Hashtable();
        ControlCollection oControlList = inControl == null ? Controls : inControl.Controls;
        for (int i = 0; i < oControlList.Count; i++)
        {
            if (oControlList[i].ID == null) continue;
            if (_lControls.ContainsKey(oControlList[i].ID))
            {
                _lControls.Remove(oControlList[i].ID);
            }

            _lControls.Add(oControlList[i].ID, oControlList[i]);
            LoadControls(oControlList[i], false);
        }
    }

    private string GetControlValue(string inControl)
    {
        return GetControlValue(FindControl(inControl));
    }

    protected virtual string GetControlValue(Control inControl)
    {
        Control oControl = inControl;
        string sValue = "";
        if (oControl is HtmlInputText) sValue = ((HtmlInputText)oControl).Value;
        if (oControl is HtmlTextArea) sValue = ((HtmlTextArea)oControl).Value;
        if (oControl is HtmlSelect) sValue = ((HtmlSelect)oControl).Value;
        if (oControl is HtmlInputCheckBox) sValue = ((HtmlInputCheckBox)oControl).Checked ? "1" : "0";
        if (oControl is HtmlInputHidden) sValue = ((HtmlInputHidden)oControl).Value;

        if (oControl is HtmlInputRadioButton)
        {
            sValue = ((HtmlInputRadioButton)oControl).Value;
            sValue = sValue == null ? "" : sValue.Trim().ToUpper();
            if (" Y YES T TRUE 1 ".IndexOf(" " + sValue + " ") >= 0) sValue = "1";
            if (" N NO F FALSE 0 ".IndexOf(" " + sValue + " ") >= 0) sValue = "0";
        }
        if (oControl is RadioButtonList)
        {
            RadioButtonList rbl = (RadioButtonList)oControl;
            for (int index = 0; index < rbl.Items.Count; index++)
            {
                if (rbl.Items[index].Selected) sValue = rbl.Items[index].Value;
            }
        }
        return sValue;
    }

    public Control? GetControlByName(string inControlName)
    {
        if (inControlName == string.Empty) return null;
        for (int i = 0; i < Controls.Count; i++)
        {
            if (Controls[i].ID == inControlName) return Controls[i];
            if (Controls[i] is HtmlControl)
            {
                if (((HtmlControl)Controls[i]).Attributes["name"] == inControlName) return Controls[i];
            }
            Control? oResult = GetControlByName(inControlName, Controls[i]);
            if (oResult != null) return oResult;
        }
        return null;
    }

    protected Control? GetControlByName(string inControlName, Control inParent)
    {
        if (inControlName == string.Empty) return null;
        for (int i = 0; i < inParent.Controls.Count; i++)
        {
            if (inParent.Controls[i].ID == inControlName) return inParent.Controls[i];
            if (inParent.Controls[i] is HtmlControl)
            {
                if (((HtmlControl)inParent.Controls[i]).Attributes["name"] == inControlName) return inParent.Controls[i];
            }
            Control? oResult = GetControlByName(inControlName, inParent.Controls[i]);
            if (oResult != null) return oResult;
        }
        return null;
    }

    protected virtual void SetControlValue(Control inControl, string inValue)
    {
        Control oControl = inControl;
        if (oControl is HtmlInputText) ((HtmlInputText)oControl).Value = inValue;
        if (oControl is HtmlTextArea) ((HtmlTextArea)oControl).Value = inValue;
        if (oControl is HtmlSelect) ((HtmlSelect)oControl).Value = inValue;
        if (oControl is HtmlInputCheckBox) ((HtmlInputCheckBox)oControl).Checked = inValue == "1";
        if (oControl is HtmlInputRadioButton) ((HtmlInputRadioButton)oControl).Value = inValue == "1" ? "Yes" : "No";
        if (oControl is RadioButtonList)
        {
            RadioButtonList rbl = (RadioButtonList)oControl;
            for (int index = 0; index < rbl.Items.Count; index++)
            {
                rbl.Items[index].Selected = inValue == rbl.Items[index].Value;
            }
        }
        if (oControl is Label)
        {
            if (inValue == "1" || inValue == "M")
            {
                ((Label)inControl).Text = "Yes";
            }
            else if (inValue == "" || inValue == "0" || inValue == "T")
            {
                ((Label)inControl).Text = "No";
            }
        }
    }
}
