using Microsoft.WindowsAzure.Storage.Table;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class EmailIndex : TableEntity
    {
        public EmailIndex() { }
        public EmailIndex(string base64Email, string userId)
        {
            PartitionKey = base64Email;
            RowKey = base64Email;
            UserId = userId;
        }

        public string UserId { get; set; }
    }
}
