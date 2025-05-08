namespace TaxReturnAutomation.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ProcessedFile> ProcessedFiles => Set<ProcessedFile>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
