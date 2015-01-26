using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using NExtensions;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class User : TableEntity, IUser
    {
        public User()
        {
            Id = Guid.NewGuid().ToString();
            Roles = string.Empty;
        }

        public string Id { get; set; }
        public string UserName {get; set;}
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string Roles { get; set; }
        public DateTime? LockoutEndDate { get; set; }
        public int AccessFailedCount { get; set; }
        public bool LockoutEnabled { get; set; }
        public string ExternalLoginsJson { get; set; }

        public List<UserLoginInfo> GetExternalLogins()
        {
            return ExternalLoginsJson.HasValue() 
                ? JsonConvert.DeserializeObject<List<UserLoginInfo>>(ExternalLoginsJson) 
                : new List<UserLoginInfo>();
        }

        private void SetExternalLogins(List<UserLoginInfo> logins)
        {
            ExternalLoginsJson = JsonConvert.SerializeObject(logins);
        }

        public void AddExternalLogin(UserLoginInfo login)
        {
            var logins = GetExternalLogins();

            var existingLogin =
                logins.FirstOrDefault(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey);

            if (existingLogin != null) return;

            logins.Add(login);
            SetExternalLogins(logins);
        }

        public void RemoveExternalLogin(UserLoginInfo login)
        {
            var logins = GetExternalLogins();

            var existingLogin =
                logins.FirstOrDefault(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey);

            logins.Remove(existingLogin);

            SetExternalLogins(logins);
        }

        public bool IsInRole(string role)
        {
            return Roles != null && Roles.Contains(role);
        }

        public void AddToRole(string role)
        {
            if (!Roles.Contains(role))
            {
                Roles.Append(string.Format(",{0}", role));
            }
        }

        public void RemoveFromRole(string role)
        {
            if (Roles.Contains(role))
            {
                var newroles = Roles.SplitByComma().ToList().Remove(role);
                Roles = string.Join(",", newroles);
            }
        }

        public void SetPartionAndRowKeys()
        {
            PartitionKey = Id;
            RowKey = Id;
        }

        public override string ToString()
        {
            return string.Format("[{0}|{1}|{2}]", UserName, Email, Id);
        }
    }
}
