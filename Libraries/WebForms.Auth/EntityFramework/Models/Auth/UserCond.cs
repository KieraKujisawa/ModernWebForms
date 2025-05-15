using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebForms.Helper.EntityFramework.Models.Auth
{
    public class UserCond
    {
        public int Id { get; set; }
        public string? Value { get; set; }
        public int AppCondId { get; set; }
        public int RoleId { get; set; }

        public UserCond(int roleId, int appCondId, string? value = null)
        {
            RoleId = roleId;
            AppCondId = appCondId;
            Value = value;
        }
    }
}
