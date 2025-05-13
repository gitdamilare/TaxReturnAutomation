namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IBankStatementParser
{
    Task<BankStatementDto> ParseAsync(
        Uri fileUri, 
        string contentType, 
        CancellationToken cancellationToken);
}
