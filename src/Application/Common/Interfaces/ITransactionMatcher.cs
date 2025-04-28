namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface ITransactionMatcher
{
    Task<IEnumerable<TransactionMatchResult>> MatchReceiptsToTransactionsAsync(
        IEnumerable<Receipt> receipts,
        IEnumerable<BankTransaction> transactions, 
        CancellationToken cancellationTokenCanc);
}
