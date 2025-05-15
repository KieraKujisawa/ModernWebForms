using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebForms.Helper.EntityFramework.Enum;

namespace WebForms.Helper.EntityFramework.Models
{
    public class UserEmail
    {
        public int EmailId { get; set; }
        public int AddressType { get; set; }
        public string Email { get; set; }
        public bool IsValidated { get; set; }
        public int UserId { get; set; }

        public UserEmail(
            int userId, 
            string email, 
            AddressType addressType = Enum.AddressType.Home
        )
        {
            UserId = userId;
            Email = email;
            AddressType = (int)addressType;
            IsValidated = false;
        }
    }
}
