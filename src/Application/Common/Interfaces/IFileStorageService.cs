namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IFileStorageService
{
    Task<byte[]> DownloadFileAsync(
        Uri fileUri,
        CancellationToken cancellationToken);
}
