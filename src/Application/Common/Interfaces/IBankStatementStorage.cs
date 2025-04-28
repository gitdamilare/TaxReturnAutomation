namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IBankStatementStorage
{
    Task SaveBankStatementAsync(BankStatement bankStatement, CancellationToken cancellationToken);
    Task<BankStatement?> GetBankStatementByIdAsync(Guid statementId, CancellationToken cancellationToken);
    Task<IEnumerable<BankTransaction>> GetAllTransactionsAsync(CancellationToken cancellationToken);
}
