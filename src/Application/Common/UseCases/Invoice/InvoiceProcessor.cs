namespace TaxReturnAutomation.Application.Common.UseCases.Invoice;
public class InvoiceProcessor : IInvoiceProcessor
{
    public Task<ProcessInvoiceResponse> ProcessAsync(ProcessInvoiceRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
