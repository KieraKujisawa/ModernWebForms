using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebForms.Helper.Enum;

namespace WebForms.Helper.Auth;

public static class UserInfo
{
    public static DataTable GetDataTable(
        Dictionary<string, DataTable> dtDict,
        UserTables tableEnum = UserTables.USER_INFO
    )
    {
        string tblName = string.Empty;

        switch (tableEnum)
        {
            case UserTables.USER_INFO:
                tblName = Const.USER_INFO;
                break;
            case UserTables.USER_ROLES:
                tblName = Const.USER_ROLES;
                break;
            case UserTables.USER_COND:
                tblName = Const.USER_COND;
                break;
            default:
                tblName = Const.USER_INFO;
                break;
        }
        if (dtDict.ContainsKey(tblName))
            return dtDict[tblName];
        else
            return new DataTable("NO_DATA");
    }

}
