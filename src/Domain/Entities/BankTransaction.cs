namespace TaxReturnAutomation.Domain.Entities;
public class BankTransaction
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public DateTime TransactionDate { get; private set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public TransactionType TransactionType { get; private set; }

    public Guid BankStatementId { get; private set; }

    private BankTransaction() { }

    public static BankTransaction Create(
        DateTime transactionDate,
        decimal amount,
        string description,
        TransactionType transactionType,
        Guid bankStatementId)
    {
        ArgumentNullException.ThrowIfNull(description);

        if (!Enum.IsDefined(transactionType))
            throw new UnsupportedTransactionTypeException(transactionType.ToString());

        return new BankTransaction
        {
            TransactionDate = transactionDate,
            Amount = amount,
            Description = description,
            TransactionType = transactionType,
            BankStatementId = bankStatementId,
        };
    }
}
