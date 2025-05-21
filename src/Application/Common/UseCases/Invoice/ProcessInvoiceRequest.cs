namespace TaxReturnAutomation.Application.Common.UseCases.Invoice;
public record ProcessInvoiceRequest(
    string FileName,
    Uri FileUri);
