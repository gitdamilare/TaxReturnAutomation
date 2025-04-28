namespace TaxReturnAutomation.Application.Common.UseCases.BankStatements;
public interface IBankStatementProcessor
{
    Task<ProcessBankStatementResponse> ProcessAsync(
        ProcessBankStatementRequest request,
        CancellationToken cancellationToken);
}
