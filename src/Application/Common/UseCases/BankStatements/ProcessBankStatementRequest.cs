namespace TaxReturnAutomation.Application.Common.UseCases.BankStatements;
public record ProcessBankStatementRequest(
    string FileName,
    Uri FileUri,
    string ContentType
);
