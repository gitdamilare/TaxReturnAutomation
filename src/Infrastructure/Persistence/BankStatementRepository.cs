namespace TaxReturnAutomation.Infrastructure.Persistence;
public class BankStatementRepository : IBankStatementRepository
{
    private readonly IApplicationDbContext _dbContext;

    public BankStatementRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task SaveBankStatementAsync(BankStatement bankStatement, CancellationToken cancellationToken)
    {
        _dbContext.BankStatements.Add(bankStatement);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public ValueTask<BankStatement?> GetBankStatementByIdAsync(Guid statementId, CancellationToken cancellationToken)
    {
        return _dbContext
            .BankStatements
            .FindAsync([statementId], cancellationToken);
    }

    public async Task<IEnumerable<BankTransaction>> GetAllTransactionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.BankStatements
            .SelectMany(bs => bs.Transactions)
            .ToListAsync(cancellationToken);
    }
}
