using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions.Triggers
{
    public class ReceiptFunction
    {
        private readonly ILogger<ReceiptFunction> _logger;

        public ReceiptFunction(ILogger<ReceiptFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ReceiptFunction))]
        public async Task Run(
            [BlobTrigger("receipts/{name}", Connection = "")] BlobClient blobClient,
            string name,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data");
            await Task.CompletedTask;
        }
    }
}
