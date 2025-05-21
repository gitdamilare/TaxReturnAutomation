
namespace TaxReturnAutomation.Infrastructure.Persistence;
public class FileProcessingTracker : IFileProcessingTracker
{
    private readonly IApplicationDbContext _dbContext;

    public FileProcessingTracker(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsFileAlreadyProcessedAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProcessedFiles
            .AnyAsync(x => x.FileName == fileName && x.Status == ProcessStatus.Completed, cancellationToken);
    }

    public async Task MarkFileAsProcessedAsync(
        string fileName,
        FileType fileType,
        ProcessStatus processStatus,
        string processingErrorMessage = "",
        CancellationToken cancellationToken = default)
    {
        if(!await IsFileAlreadyProcessedAsync(fileName, cancellationToken))
        {
            var processedFile = ProcessedFile.Create(fileName, fileType, processStatus, processingErrorMessage);
            _dbContext.ProcessedFiles.Add(processedFile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
