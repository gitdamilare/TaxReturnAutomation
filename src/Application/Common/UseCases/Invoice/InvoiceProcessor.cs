using Microsoft.Extensions.Logging;
using TaxReturnAutomation.Application.Common.Mapper;
using TaxReturnAutomation.Domain.Enums;

namespace TaxReturnAutomation.Application.Common.UseCases.Invoice;
public class InvoiceProcessor : IInvoiceProcessor
{
    private readonly ILogger<InvoiceProcessor> _logger;
    private readonly IFileProcessingTracker _fileProcessingTracker;
    private readonly IInvoiceStorage _invoiceRepository;
    private readonly IInvoiceParser _invoiceParser;

    public InvoiceProcessor(
        IInvoiceStorage invoiceRepository,
        IInvoiceParser invoiceParser,
        IFileProcessingTracker fileProcessingTracker,
        ILogger<InvoiceProcessor> logger)
    {
        _invoiceRepository = invoiceRepository;
        _invoiceParser = invoiceParser;
        _fileProcessingTracker = fileProcessingTracker;
        _logger = logger;
    }


    public async Task<ProcessInvoiceResponse?> ProcessAsync(
        ProcessInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (await _fileProcessingTracker.IsFileAlreadyProcessedAsync(request.FileName, cancellationToken))
        {
            _logger.LogInformation("Skipping already processed file: {BlobName}", request.FileName);
            return default;
        }

        try
        {
            var invoiceDto = await _invoiceParser.ParseAsync(request.FileUri, cancellationToken);

            var isValid = invoiceDto.ProcessingErrorMessages.Count == 0;
            var processingErrorMessage = string.Join(", ", invoiceDto.ProcessingErrorMessages);
            await TrackProcessingResultAsync(
                request.FileName,
                isValid ? ProcessStatus.Completed : ProcessStatus.Failed,
                processingErrorMessage,
                cancellationToken);

            if (!isValid)
            {
                _logger.LogWarning(
                    "Invoice validation failed for file: {BlobName}. Errors: {Errors}",
                    request.FileName,
                    processingErrorMessage);
                return default;
            }

            var invoice = InvoiceMapper.MapFromDto(invoiceDto);
            await _invoiceRepository.SaveInvoiceAsync(invoice, cancellationToken);

            _logger.LogInformation("Invoice processed and saved successfully for file: {BlobName}", request.FileName);
            return new ProcessInvoiceResponse(
                InvoiceId: invoice.InvoiceNumber,
                CustomerId: invoice.CustomerId,
                CustomerName: invoice.CustomerName,
                InvoiceDate: invoice.PurchaseDate,
                TotalAmount: invoice.TotalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing invoice file: {BlobName}", request.FileName);
            await TrackProcessingResultAsync(request.FileName, ProcessStatus.Failed, $"Error processing invoice file: {request.FileName}", cancellationToken);
            throw;
        }
    }

    private Task TrackProcessingResultAsync(
        string fileName,
        ProcessStatus status,
        string validationErrorMesage,
        CancellationToken cancellationToken)
    {
        return _fileProcessingTracker.MarkFileAsProcessedAsync(
            fileName,
            FileType.Invoice,
            status,
            validationErrorMesage,
            cancellationToken);
    }
}
