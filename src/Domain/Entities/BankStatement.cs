namespace TaxReturnAutomation.Domain.Entities;
public class BankStatement
{
    private readonly List<BankTransaction> _transactions = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FileName { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public IReadOnlyList<BankTransaction> Transactions => _transactions.AsReadOnly();

    private BankStatement() { }

    public static BankStatement Create(string? fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        return new BankStatement
        {
            FileName = fileName,
            UploadedAt = DateTime.UtcNow
        };
    }

    public void AddTransaction(BankTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        _transactions.Add(transaction);
    }
}
