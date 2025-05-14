using TaxReturnAutomation.Domain.Enums;

namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IFileProcessingTracker
{
    Task<bool> IsFileAlreadyProcessedAsync(string fileName, CancellationToken cancellationToken = default);
    Task MarkFileAsProcessedAsync(string fileName, FileType fileType, ProcessStatus processStatus, CancellationToken cancellationToken = default);
}
