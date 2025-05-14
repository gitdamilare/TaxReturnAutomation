using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TaxReturnAutomation.Application.Common.Interfaces;
using TaxReturnAutomation.Application.Common.UseCases.BankStatements;
using TaxReturnAutomation.Application.Common.UseCases.Invoice;
using TaxReturnAutomation.Domain.Enums;

namespace Functions.Triggers
{
    public class InvoiceFunction
    {
        private readonly ILogger<InvoiceFunction> _logger;
        private readonly IFileProcessingTracker _fileProcessingTracker;
        private readonly IInvoiceProcessor _invoiceProcessor;

        public InvoiceFunction(
            ILogger<InvoiceFunction> logger,
            IFileProcessingTracker fileProcessingTracker,
            IInvoiceProcessor invoiceProcessor)
        {
            _logger = logger;
            _fileProcessingTracker = fileProcessingTracker;
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
                if (await _fileProcessingTracker.IsFileAlreadyProcessedAsync(name, cancellationToken))
                {
                    _logger.LogInformation("Skipping already processed file: {BlobName}", name);
                    return;
                }

                //Process the blob
                var request = new ProcessInvoiceRequest(name, blobClient.Uri);
                var result = await _invoiceProcessor.ProcessAsync(request, cancellationToken);

                _logger.LogInformation("Processed file: {BlobName} with result: {Result} and Transcation Count", name, result, result.TransactionCount);
                await _fileProcessingTracker.MarkFileAsProcessedAsync(name, FileType.BankStatement, ProcessStatus.Completed, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {BlobName}", name);
                await _fileProcessingTracker.MarkFileAsProcessedAsync(name, FileType.BankStatement, ProcessStatus.Failed, cancellationToken);
                throw;
            }
        }
    }
}
