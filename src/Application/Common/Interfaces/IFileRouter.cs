namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IFileRouter
{
    Task RouteFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken);
}
