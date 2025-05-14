using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TaxReturnAutomation.Application.Common.Interfaces;
using TaxReturnAutomation.Application.Common.UseCases.BankStatements;
using TaxReturnAutomation.Domain.Enums;

namespace Functions.Triggers
{
    public class BankStatementFunction
    {
        private readonly ILogger<BankStatementFunction> _logger;
        private readonly IFileProcessingTracker _fileProcessingTracker;
        private readonly IBankStatementProcessor _bankStatementProcessor;

        public BankStatementFunction(
            ILogger<BankStatementFunction> logger,
            IFileProcessingTracker fileProcessingTracker,
            IBankStatementProcessor bankStatementProcessor)
        {
            _logger = logger;
            _fileProcessingTracker = fileProcessingTracker;
            _bankStatementProcessor = bankStatementProcessor;
        }

        [Function(nameof(BankStatementFunction))]
        public async Task Run(
            [BlobTrigger("bankstatements/{name}", Connection = "AzureWebJobsStorage")] BlobClient blobClient,
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
                var request = new ProcessBankStatementRequest(name, blobClient.Uri, string.Empty);
                var result = await _bankStatementProcessor.ProcessAsync(request, cancellationToken);

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
