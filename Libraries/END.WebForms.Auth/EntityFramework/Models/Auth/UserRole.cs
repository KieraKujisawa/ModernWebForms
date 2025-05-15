using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebForms.Helper.EntityFramework.Models.Auth
{
    public class UserRole
    {
        public int Id { get; set; }
        public int AppRoleId { get; set; }
        public List<UserCond>? Conditions { get; set; }
        public int UserId { get; set; }

        public UserRole(int userId, int appRoleId)
        {
            UserId = userId;
            AppRoleId = appRoleId;
        }


    }
}
