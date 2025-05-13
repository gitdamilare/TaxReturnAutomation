namespace TaxReturnAutomation.Domain.Entities;
public class Invoice
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FileName { get; private set; } = string.Empty;
    public string InvoiceNumber { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private Invoice() { }

    public static Invoice Create(
        string fileName,
        decimal amount,
        string description,
        string InvoiceNumber,
        string customerName,
        DateTime purchaseDate)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        return new Invoice
        {
            FileName = fileName,
            UploadedAt = DateTime.UtcNow,
            TotalAmount = amount,
            Description = description,
            InvoiceNumber = InvoiceNumber,
            CustomerName = customerName,
            PurchaseDate = purchaseDate
        };
    }
}
