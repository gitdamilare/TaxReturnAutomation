using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions.Triggers
{
    public class InvoiceFunction
    {
        private readonly ILogger<InvoiceFunction> _logger;

        public InvoiceFunction(ILogger<InvoiceFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(InvoiceFunction))]
        public async Task Run(
            [BlobTrigger("invoices/{name}", Connection = "")] BlobClient blobClient,
            string name,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {name} \n Data");
            await Task.CompletedTask;
        }
    }
}
