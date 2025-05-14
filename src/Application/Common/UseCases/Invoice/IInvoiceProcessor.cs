namespace TaxReturnAutomation.Application.Common.UseCases.Invoice;

public interface IInvoiceProcessor
{
    Task<ProcessInvoiceResponse> ProcessAsync(
        ProcessInvoiceRequest request,
        CancellationToken cancellationToken);
}
