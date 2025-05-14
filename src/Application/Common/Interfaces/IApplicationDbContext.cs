namespace TaxReturnAutomation.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<BankStatement> BankStatements { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<ProcessedFile> ProcessedFiles { get; }
    DbSet<MatchResult> MatchResults { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
