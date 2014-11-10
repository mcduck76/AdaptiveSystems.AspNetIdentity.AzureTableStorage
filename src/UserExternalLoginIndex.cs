using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using NExtensions;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class UserExternalLoginIndex : TableEntity
    {
        public UserExternalLoginIndex() { }
        public UserExternalLoginIndex(UserLoginInfo login) : this(login, string .Empty) { }
        public UserExternalLoginIndex(UserLoginInfo login, string userId)
        {
            PartitionKey = login.LoginProvider.Base64Encode();
            RowKey = login.ProviderKey.Base64Encode();
            UserId = userId;
        }

        public string UserId { get; set; }
    }
}