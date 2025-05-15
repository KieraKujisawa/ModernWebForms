using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebForms.Helper.EntityFramework.Models.Application
{
    public class App
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public List<AppRole>? Roles { get; set; }

        public App()
        {
            Name = "";
            IsActive = true;
            IsOnline = true;
        }

        public App(string name)
        {
            Name = name;
            IsActive = true;
            IsOnline = true;
        }
    }
}
