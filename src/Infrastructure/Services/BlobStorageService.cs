namespace TaxReturnAutomation.Infrastructure.Services;
public class BlobStorageService : IFileStorageService
{
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(ILogger<BlobStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> DownloadFileAsync(Uri fileUri, CancellationToken cancellationToken)
    {
        try
        {
            var blobClient = new BlobClient(fileUri);
            var response = await blobClient.DownloadAsync(cancellationToken: cancellationToken);
            using var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download blob from {BlobUri}", fileUri);
            throw;
        }
    }
}
