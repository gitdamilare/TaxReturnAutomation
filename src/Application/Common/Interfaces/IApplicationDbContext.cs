using TaxReturnAutomation.Domain.Entities;

namespace TaxReturnAutomation.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
