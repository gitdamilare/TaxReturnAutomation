namespace TaxReturnAutomation.Application.Common.DTOs;
public class InvoiceDto
{
    public string FileName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TotalTax 
    { 
        get;
        set
        {
            if(value < 0)
            {
               if(SubTotal > 0 && TotalTax > 0)
                {
                    field = SubTotal + TotalTax;
                }
            }
        } 
    }
    public decimal TotalAmount { get; set; }
    public string Description { get; set; } = string.Empty;

    public InvoiceDto(string fileName)
    {
        FileName = fileName;
    }
}
