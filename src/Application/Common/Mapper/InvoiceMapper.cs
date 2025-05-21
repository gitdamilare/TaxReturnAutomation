namespace TaxReturnAutomation.Application.Common.Mapper;
public class InvoiceMapper
{
    public static Invoice MapFromDto(
        InvoiceDto invoiceDto)
    {
        var invoice = Invoice.Create(
            fileName: invoiceDto.FileName,
            amount: invoiceDto.TotalAmount,
            description: invoiceDto.Description,
            invoiceNumber: invoiceDto.InvoiceNumber,
            customerId: invoiceDto.CustomerId,
            customerName: invoiceDto.CustomerName,
            purchaseDate: invoiceDto.PurchaseDate);

        return invoice;
    }
}
