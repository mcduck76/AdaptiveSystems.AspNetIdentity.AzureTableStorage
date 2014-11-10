using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Table;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class User : TableEntity, IUser
    {
        public User()
        {
            Id = Guid.NewGuid().ToString();
            ExternalLogins = new List<UserLoginInfo>();
        }
        public User(string username) : this()
        {
            UserName = username;
        }

        public string Id { get; set; }
        public string UserName {get; set;}
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string[] Roles { get; set; }
        public DateTime? LockoutEndDate { get; set; }
        public int AccessFailedCount { get; set; }
        public bool LockoutEnabled { get; set; }
        public IList<UserLoginInfo> ExternalLogins { get; set; }

        public bool IsUserInRole(string role)
        {
            return Roles != null && Roles.Contains(role);
        }

        public void SetPartionAndRowKeys()
        {
            PartitionKey = Id;
            RowKey = Id;
        }

    }
}
