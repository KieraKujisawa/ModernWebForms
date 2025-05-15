using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebForms.Helper.EntityFramework.Models.Application
{
    public class AppRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public List<AppCond>? Conditions { get; set; }

        public int AppId { get; set; }

        public AppRole()
        {
            Name = "";
            AppId = -1;
        }

        public AppRole(int appId, string name)
        {
            AppId = appId;
            Name = name;
        }
    }
}
