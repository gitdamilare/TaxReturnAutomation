
namespace TaxReturnAutomation.Infrastructure.Persistence;
internal class InvoiceRepository : IInvoiceStorage
{
    private readonly IApplicationDbContext _dbContext;

    public InvoiceRepository(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ValueTask<Invoice?> GetInvoiceByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Invoices.FindAsync([id], cancellationToken);
    }

    public Task<Invoice?> GetInvoiceByFileNameAsync(string fileName, CancellationToken cancellationToken)
    {
        return _dbContext.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.FileName == fileName, cancellationToken);
    }

    public Task<List<Invoice>> GetAllInvoicesAsync()
    {
        return _dbContext.Invoices
            .AsNoTracking()
            .ToListAsync();
    }

    public Task SaveInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        _dbContext.Invoices.Add(invoice);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
