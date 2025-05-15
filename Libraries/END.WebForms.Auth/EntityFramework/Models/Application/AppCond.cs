using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebForms.Helper.EntityFramework.Models.Application
{
    public class AppCond
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; }

        public AppCond()
        {
            Name = string.Empty;
            RoleId = -1;
        }

        public AppCond(int roleId, string name)
        {
            RoleId = roleId;
            Name = name;
        }

    }
}
