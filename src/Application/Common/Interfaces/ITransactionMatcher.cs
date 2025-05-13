namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface ITransactionMatcher
{
    Task<IEnumerable<TransactionMatchResult>> MatchInvoicesToTransactionsAsync(
        IEnumerable<Invoice> invoices,
        IEnumerable<BankTransaction> transactions, 
        CancellationToken cancellationTokenCanc);
}
