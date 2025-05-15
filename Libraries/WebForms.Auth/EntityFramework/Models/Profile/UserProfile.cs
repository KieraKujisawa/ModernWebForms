using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Security;

namespace WebForms.Helper.EntityFramework.Models
{
    public class UserProfile
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public List<UserAddress>? Addresses { get; set; }
        public List<UserEmail>? Emails { get; set; }

        public UserProfile (string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

    }
}
