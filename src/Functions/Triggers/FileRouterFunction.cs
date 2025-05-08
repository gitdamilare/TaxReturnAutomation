using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions.Triggers
{
    public class FileRouterFunction
    {
        private readonly ILogger<FileRouterFunction> _logger;

        public FileRouterFunction(ILogger<FileRouterFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(FileRouterFunction))]
        public async Task FileRouter([BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
        {
            using var blobStreamReader = new StreamReader(stream);
            var content = await blobStreamReader.ReadToEndAsync();
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}");
        }

        [Function("Router")]
        public async Task Router([BlobTrigger("samples-workitem/{name}", Connection = "AzureWebJobsStorage")] BlobClient blobClient,
            string name)
        {
            var client = blobClient.AccountName.ToLower();
            _logger.LogInformation($"{name}, {client}");
            await Task.CompletedTask;
        }
    }
}
