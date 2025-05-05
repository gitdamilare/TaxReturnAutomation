namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IFileStorageService
{
    Task<byte[]> DownloadFileAsync(
        string blobUri,
        CancellationToken cancellationToken);
}
