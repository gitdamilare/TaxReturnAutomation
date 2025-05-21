using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TaxReturnAutomation.Application.Common.UseCases.Invoice;

namespace Functions.Triggers
{
    public class InvoiceFunction
    {
        private readonly ILogger<InvoiceFunction> _logger;
        private readonly IInvoiceProcessor _invoiceProcessor;

        public InvoiceFunction(
            ILogger<InvoiceFunction> logger,
            IInvoiceProcessor invoiceProcessor)
        {
            _logger = logger;
            _invoiceProcessor = invoiceProcessor;
        }

        [Function(nameof(InvoiceFunction))]
        public async Task Run(
            [BlobTrigger("invoices/{name}", Connection = "")] BlobClient blobClient,
            string name,
            CancellationToken cancellationToken)
        {
            try
            {
                var request = new ProcessInvoiceRequest(name, blobClient.Uri);
                var result = await _invoiceProcessor.ProcessAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {BlobName}", name);
                throw;
            }
        }
    }
}
