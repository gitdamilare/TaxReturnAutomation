namespace TaxReturnAutomation.Application.Common.UseCases.BankStatements;
public record ProcessBankStatementRequest(
    string FileName,
    string BlobUri,
    string ContentType
);
