using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzFuncApp
{
    public static class queueNordine
    {
        [FunctionName("queueNordine")]
        public static void Run([QueueTrigger("myqueue-items", Connection = "StorageConnectionAppSetting")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
