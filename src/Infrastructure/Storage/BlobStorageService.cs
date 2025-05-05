namespace TaxReturnAutomation.Infrastructure.Storage;
public class BlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<byte[]> DownloadFileAsync(string blobUri, CancellationToken cancellationToken)
    {
        try
        {
            var blobClient = new BlobClient(new Uri(blobUri));
            var response = await blobClient.DownloadAsync(cancellationToken: cancellationToken);
            using var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download blob from {BlobUri}", blobUri);
            throw;
        }
    }
}
