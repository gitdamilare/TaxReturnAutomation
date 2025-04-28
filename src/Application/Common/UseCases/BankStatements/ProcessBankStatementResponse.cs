namespace TaxReturnAutomation.Application.Common.UseCases.BankStatements;
public record ProcessBankStatementResponse(
    Guid BankStatementId,
    int TransactionCount);
