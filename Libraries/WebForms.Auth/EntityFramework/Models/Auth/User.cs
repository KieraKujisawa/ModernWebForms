using System;
using System.Collections.Generic;
using BNet = BCrypt.Net;

namespace WebForms.Helper.EntityFramework.Models.Auth
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Credential { get; set; }

        public List<UserRole>? Roles { get; set; }

        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        
        public User()
        {
            Name = string.Empty;
            IsActive = false;
            IsAdmin = false;
        }

        public User(string userName, string cred, bool isActive = true, bool isAdmin = false)
        {
            Name = userName;
            Credential = GetCredentialHash(cred);
            IsActive = isActive;
            IsAdmin = isAdmin;
        }

        private string GetCredentialHash(string cred)
        {
            return BNet.BCrypt.HashPassword(cred);
        }

        public static bool ValidateCred(string cred, string hash)
        {
            return BNet.BCrypt.Verify(cred, hash);
        }
    }
}
