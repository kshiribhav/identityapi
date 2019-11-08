using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;

namespace IdentityAPI.Helpers
{
    public class StorageAccountHelper
    {
        private string storageConnectionString;
        private CloudStorageAccount cloudStorageAccount;
        private CloudQueueClient cloudQueueClient;

        public string StorageConnectionString
        {
            get { return storageConnectionString; }
            set
            {
                this.storageConnectionString = value;
                cloudStorageAccount = CloudStorageAccount.Parse(this.storageConnectionString);
            }
        }

        public async Task SendMessageAsync(string message, string queueName)
        {
            cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();
            var queue = cloudQueueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            CloudQueueMessage queueMessage = new CloudQueueMessage(message);
            await queue.AddMessageAsync(queueMessage);
        }
    }
}
