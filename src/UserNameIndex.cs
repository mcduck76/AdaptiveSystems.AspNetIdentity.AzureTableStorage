using Microsoft.WindowsAzure.Storage.Table;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class UserNameIndex : TableEntity
    {
        public UserNameIndex() { }
        public UserNameIndex(string base64UserName, string userId)
        {
            PartitionKey = base64UserName;
            RowKey = base64UserName;
            UserId = userId;
        }

        public string UserId { get; set; }
    }
}
