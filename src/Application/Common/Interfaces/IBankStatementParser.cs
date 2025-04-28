namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IBankStatementParser
{
    Task<BankStatement> ParseAsync(
        string blobUri, 
        string contentType, 
        CancellationToken cancellationToken);
}
