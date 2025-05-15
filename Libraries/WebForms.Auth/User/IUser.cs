using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace WebForms.Helper.User
{
    public interface IUser
    {
        string UserId { get; }
        SecureString? Credential { get; }
        string UserKey { get; }
        Dictionary<string, string> UserInfo { get; }
        Hashtable? UserRoles { get; } // k/v=role/conditions, k/v=condition/assignedvalues, k/v=assignedvalue/"".
        bool HasRole(string inRole);
        bool HasRoleInContext(string inRole, Hashtable inContext); // k/v=condition/context
        Hashtable? GetUserConditions();                       // k/v=anycondition/assignedvalues, k/v=assignedvalue/"".
        Hashtable? GetUserConditionsForRole(string inRole); // k/v=rolecondition/assignedvalues, k/v=assignedvalue/"".
        Hashtable? GetUserValues(string inCondition);
    }
}
