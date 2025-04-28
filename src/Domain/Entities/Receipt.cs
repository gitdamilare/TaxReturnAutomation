namespace TaxReturnAutomation.Domain.Entities;
public class Receipt
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string FileName { get; private set; } = string.Empty;
    public string ReceiptNumber { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public DateTime PurchaseDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    private Receipt() { }
    public static Receipt Create(
        string fileName,
        decimal amount,
        string description,
        DateTime purchaseDate)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        return new Receipt
        {
            FileName = fileName,
            UploadedAt = DateTime.UtcNow,
            TotalAmount = amount,
            Description = description,
            PurchaseDate = purchaseDate
        };
    }
}
