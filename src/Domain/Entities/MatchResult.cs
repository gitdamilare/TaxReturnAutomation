namespace TaxReturnAutomation.Domain.Entities;
public class MatchResult
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public decimal MatchedAmount { get; private set; }
    public DateTime MatchedAt { get; private set; }
    public MatchConfidence MatchConfidence { get; private set; }

    public Guid ReceiptId { get; private set; }
    public Receipt? Receipt { get; private set; }

    public Guid BankTransactionId { get; private set; }
    public BankTransaction? BankTransaction { get; private set; }

    private MatchResult() { }

    public static MatchResult Create(
        Guid receiptId,
        Guid bankTransactionId,
        decimal matchedAmount,
        MatchConfidence matchConfidence)
    {
        return new MatchResult
        {
            ReceiptId = receiptId,
            BankTransactionId = bankTransactionId,
            MatchedAmount = matchedAmount,
            MatchedAt = DateTime.UtcNow,
            MatchConfidence = matchConfidence
        };
    }
}
