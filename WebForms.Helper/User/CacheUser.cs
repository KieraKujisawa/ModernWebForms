using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Runtime.InteropServices;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web.SessionState;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using WebForms.Helper.Enum;

#pragma warning disable CS8618


namespace WebForms.Helper.User;

[DataContract]
[Serializable]
public class CacheUser : IUser
{
    #region "Vars / Const"

    private const string _userKey = "USER_SEQ";
    private readonly string _userId;
    [NonSerialized]
    private SecureString? _cred;
    private readonly int _userSeq;

    private readonly bool _isValid = true;
    private readonly Dictionary<string, string> _userDetail = new();
    private readonly List<Dictionary<string, string>> _roleInfo = new();
    private readonly List<Dictionary<string, string>> _condInfo = new();

    private readonly Hashtable _roles = new();

    #endregion

    #region "Properties"

    [JsonIgnore]
    public bool IsValid => _isValid;

    [DataMember]
    public string UserId => _userId;

    [JsonIgnore]
    public SecureString? Credential => _cred;

    [DataMember]
    public string UserKey => _userSeq.ToString();

    [JsonIgnore]
    public int UserSeq => _userSeq;

    [DataMember]
    public Dictionary<string, string> UserInfo => _userDetail;

    [DataMember]
    public Hashtable? UserRoles => GetUserRoles();

    #endregion

    #region "Constructors"

    public CacheUser(IConfigurationRoot? config)
    {
        if (config == null) return;
        foreach (var entry in config.GetChildren())
        {
            switch (entry.Key)
            {
                case "UserInfo":
                    foreach (var userDict in entry.GetChildren())
                    {
                        _userDetail.Add(userDict.Key, ToStr(userDict.Value));
                    }
                    break;
                case "UserId":
                    _userId = ToStr(entry.Value);
                    break;
                case "UserKey":
                    int.TryParse(ToStr(entry.Value), out _userSeq);
                    break;
                case "UserRoles":
                    foreach (var sRole in entry.GetChildren())
                    {
                        var listValue = new List<string>();
                        var dictCond = new Dictionary<string, List<string>?>();
                        foreach (var sCond in sRole.GetChildren())
                        {
                            foreach (var sValue in sCond.GetChildren())
                            {
                                listValue.Add(ToStr(sValue.Value));
                            }
                            if (listValue.Count > 0)
                            {
                                dictCond.Add(sCond.Key, listValue);
                            }
                            else
                            {
                                dictCond.Add(sCond.Key, null);
                            }
                        }
                        _roleInfo.Add(GetDict("ROLE", sRole.Key));
                        if (dictCond.Count == 0) continue;
                        {
                            _condInfo = GetListDictCond(sRole.Key, dictCond);
                        }
                    }
                    _roles = GetUserRoles();
                    break;
                default:
                    break;
            }
        }
    }

    


    public CacheUser(string userId, Dictionary<string, DataTable> userDict)
    {
        _userId = userId;

        var dt = Auth.UserInfo.GetDataTable(userDict, UserTables.USER_DETAIL);
        var listDict = ToListDict(dt);
        _userDetail = listDict[0];
        dt = Auth.UserInfo.GetDataTable(userDict, UserTables.USER_ROLES);
        _roleInfo = ToListDict(dt);
        dt = Auth.UserInfo.GetDataTable(userDict, UserTables.USER_COND);
        _condInfo = ToListDict(dt);

        if (!int.TryParse(_userDetail[_userKey], out _userSeq))
            _userSeq = -1;

        _roles = GetUserRoles();
    }

    #endregion

    #region "IUser Functions"

    public Hashtable? GetUserConditions()
    {
        return GetUserCond();
    }

    public Hashtable? GetUserConditionsForRole(string inRole)
    {
        Hashtable? htCond = null;
        if (_roles.ContainsKey(inRole))
            htCond = (Hashtable?)_roles[inRole];
        return htCond;
    }

    public Hashtable? GetUserValues(string inCondition)
    {
        var ret = new Hashtable();
        var htCond = GetUserCond();

        if (htCond.ContainsKey(inCondition))
        {
            var value = htCond[inCondition];
        }
        return ret;
    }

    public bool HasRole(string inRole)
    {
        return _roles.ContainsKey(inRole);
    }

    public bool HasRoleInContext(string inRole, Hashtable inContext)
    {
        return inContext.ContainsKey(inRole);
    }

    #endregion

    #region "Helper Functions"

    public void SetCred(string cred)
    {
        _cred = ConvertToSecureString(cred);
    }

    [DebuggerStepThrough]
    private HttpSessionState? GetSession()
    {
        var context = HttpContext.Current;
        if (context == null) return null;
        return context.Session;
    }

    [DebuggerStepThrough]
    private Hashtable GetUserRoles()
    {
        if (_roleInfo.Count == 0)
            return new Hashtable();

        var htRoles = new Hashtable();
        var htCond = GetUserCond();

        var listRoles = new List<string>();
        var listCond = new List<string>();
        var dictRoles = new Dictionary<string, List<string>>();
        var dt = ToDataTable(_condInfo);

        foreach (var entry in _roleInfo)
        {
            var role = entry["ROLE"];
            if (!listRoles.Contains(role))
                listRoles.Add(role);
        }

        if (_condInfo.Count > 0)
        {
            foreach (var role in listRoles)
            {
                var dv = dt.DefaultView;
                dv.RowFilter = $"ROLE = '{role}'";
                if (dv.Count == 0) continue;
                listCond.Clear();
                for (int i = 0; i < dv.Count; i++)
                {
                    var rw = dv[i];
                    if (rw == null) continue;
                    string? cond = (string?)rw["COND_TITLE"];
                    if (cond == null) continue;
                    if (!listCond.Contains(cond))
                        listCond.Add(Convert.ToString(cond));
                }
                dictRoles.Add(role, listCond);
            }

            foreach (var role in listRoles)
            {
                if (!dictRoles.ContainsKey(role))
                {
                    htRoles.Add(role, null);
                    continue;
                }
                var condList = dictRoles[role];
                var retCond = new Hashtable();
                foreach (var cond in condList)
                {
                    if (htCond.ContainsKey(cond))
                        retCond.Add(cond, htCond[cond]);
                }
                if (retCond.Count == 0)
                    htRoles.Add(role, null);
                else
                    htRoles.Add(role, retCond);
            }
        }
        else
        {
            foreach (var role in listRoles)
            {
                htRoles.Add(role, null);
            }
        }
        return htRoles;
    }

    [DebuggerStepThrough]
    private Hashtable GetUserCond()
    {
        const string condTitle = "COND_TITLE";
        const string condValue = "COND_VALUE";

        var ret = new Hashtable();
        var listCond = new List<string>();
        var listValue = new List<string>();

        foreach (var entry in _condInfo)
        {
            var cond = entry[condTitle];
            if (!listCond.Contains(cond))
                listCond.Add(cond);
        }

        foreach (var cond in listCond)
        {
            listValue.Clear();
            foreach (var entry in _condInfo)
            {
                if (cond != entry[condTitle])
                    continue;
                var value = entry[condValue];
                if (!listValue.Contains(value))
                    listValue.Add(value);
            }
            ret.Add(cond, listValue.ToArray());
        }

        return ret;
    }

    [DebuggerStepThrough]
    public List<Dictionary<string, string>> GetListDictCond
    (
    string role,
    Dictionary<string, List<string>?> dictCond
    )
    {
        var listDict = new List<Dictionary<string, string>>();

        foreach (var cond in dictCond)
        {
            if (cond.Value == null)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("ROLE", role);
                dict.Add("COND_TITLE", cond.Key);
                dict.Add("COND_VALUE", string.Empty);
                listDict.Add(dict);
                continue;
            }

            foreach (var item in cond.Value)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("ROLE", role);
                dict.Add("COND_TITLE", cond.Key);
                dict.Add("COND_VALUE", item);
                listDict.Add(dict);
            }
        }

        return listDict;
    }

    #endregion

    #region "Util Functions"

    [DebuggerStepThrough]
    private static SecureString ConvertToSecureString(string cred)
    {
        if (cred == null)
            throw new ArgumentNullException("cred");

        var secureCred = new SecureString();

        foreach (char c in cred)
            secureCred.AppendChar(c);

        secureCred.MakeReadOnly();
        return secureCred;
    }

    private static string? SecureStringToString(SecureString value)
    {
        IntPtr valuePtr = IntPtr.Zero;
        try
        {
            valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
            return Marshal.PtrToStringUni(valuePtr);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
        }
    }

    [DebuggerStepThrough]
    private static string StrFromObj(object? val)
    {
        if (val == null) return string.Empty;
        string? ret = Convert.ToString(val);
        return string.IsNullOrEmpty(ret) ? string.Empty : ret;
    }

    [DebuggerStepThrough]
    private static string ToStr(string? val)
    {
        return string.IsNullOrEmpty(val) ? string.Empty : Convert.ToString(val);
    }

    [DebuggerStepThrough]
    private static DataTable ToDataTable(List<Dictionary<string, string>> inData)
    {
        DataTable dt = new DataTable();

        if (inData.Count > 0)
        {
            var dict = inData[0];
            foreach (var key in dict.Keys)
            {
                dt.Columns.Add(key);
            }
        }

        dt.BeginLoadData();
        foreach (var entry in inData)
        {
            var dr = dt.NewRow();
            foreach (var key in entry.Keys)
            {
                dr[key] = entry[key];
            }
            dt.Rows.Add(dr);
        }
        dt.EndLoadData();

        return dt;
    }

    [DebuggerStepThrough]
    private static List<Dictionary<string, string>> ToListDict(DataTable dt)
    {
        var listDict = new List<Dictionary<string, string>>();

        foreach (DataRow dr in dt.Rows)
        {
            var dict = new Dictionary<string, string>();
            foreach (DataColumn dc in dt.Columns)
            {
                dict.Add(dc.ColumnName, StrFromObj(dr[dc]));
            }
            listDict.Add(dict);
        }

        return listDict;
    }

    private static Dictionary<string, string> GetDict(string key, string? value)
    {
        var ret = new Dictionary<string, string>();
        ret.Add(key, ToStr(value));
        return ret;
    }

    #endregion

    #region "Json Functions"

    public string ToJson()
    {
        var options = new JsonSerializerOptions();
        options.WriteIndented = true;
        var jsonString = System.Text.Json.JsonSerializer.Serialize(this, options);
        return jsonString;
    }

    public static CacheUser? FromJson(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
            return null;
        CacheUser userSettings;

        using (var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
        {
            var config = new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();

            userSettings = new(config);
        }

        return userSettings;
    }

    #endregion

}
