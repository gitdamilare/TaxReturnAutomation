namespace TaxReturnAutomation.Application.Common.UseCases.Invoice;
public class ProcessInvoiceResponse
{
    public string InvoiceId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAmount { get; set; }
}
