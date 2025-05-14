namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IInvoiceParser
{
    Task<InvoiceDto> ParseAsync(
        Uri fileUri,
        CancellationToken cancellationToken);
}
