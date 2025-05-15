using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using WebForms.Helper.EntityFramework.Enum;

namespace WebForms.Helper.EntityFramework.Models
{
    public class UserAddress
    {
        public int UserAddressId { get; set; }
        public int AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public int UserId { get; set; }

        public UserAddress(
            int userId,
            string line1, string? line2, string? line3,
            string city, string postalCode,
            string state, string country,
            AddressType addressType = Enum.AddressType.Home
            )
        {
            UserId = userId;
            AddressLine1 = line1;
            AddressLine2 = line2;
            AddressLine3 = line3;
            City = city;
            PostalCode = postalCode;
            State = state;
            Country = country;
            AddressType = (int)addressType;
        }
    }
}
