namespace TaxReturnAutomation.Application.Common.Mapper;
public class InvoiceMapper
{
    public static Invoice MapFromDto(
        InvoiceDto invoiceDto)
    {
        var description = invoiceDto.ValidationErrors.Count != 0
            ? string.Join(", ", invoiceDto.ValidationErrors)
            : invoiceDto.Description;

        var invoice = Invoice.Create(
            fileName: invoiceDto.FileName,
            amount: invoiceDto.TotalAmount,
            description: description,
            invoiceNumber: invoiceDto.InvoiceNumber,
            customerId: invoiceDto.CustomerId,
            customerName: invoiceDto.CustomerName,
            purchaseDate: invoiceDto.PurchaseDate);

        return invoice;
    }
}
