namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IBankStatementRepository
{
    Task SaveBankStatementAsync(BankStatement bankStatement, CancellationToken cancellationToken);
    ValueTask<BankStatement?> GetBankStatementByIdAsync(Guid statementId, CancellationToken cancellationToken);
    Task<IEnumerable<BankTransaction>> GetAllTransactionsAsync(CancellationToken cancellationToken);
}
