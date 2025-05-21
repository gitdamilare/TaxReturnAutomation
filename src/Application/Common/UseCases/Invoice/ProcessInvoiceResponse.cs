namespace TaxReturnAutomation.Application.Common.UseCases.Invoice;

public record ProcessInvoiceResponse(
    string InvoiceId,
    string CustomerId,
    string CustomerName,
    DateTime InvoiceDate,
    decimal TotalAmount);
