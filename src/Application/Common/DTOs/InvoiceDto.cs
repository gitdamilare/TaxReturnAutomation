namespace TaxReturnAutomation.Application.Common.DTOs;
public class InvoiceDto
{
    public string FileName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.MinValue;
    public decimal SubTotal { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalAmount
    {
        get => (SubTotal > 0 && TotalTax > 0) ? SubTotal + TotalTax : field; 
        set;
    }
    public string Description { get; set; } = string.Empty;

    public List<string> ProcessingErrorMessages { get; set; } = [];

    public InvoiceDto(string fileName)
    {
        FileName = fileName;
    }
}
