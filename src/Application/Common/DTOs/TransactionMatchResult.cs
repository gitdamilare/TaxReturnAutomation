namespace TaxReturnAutomation.Application.Common.DTOs;

//TODO: Add Properties 
public record TransactionMatchResult(
    Guid TransactionId,
    Guid ReceiptId,
    string ReceiptName,
    string ReceiptBlobUri,
    DateTime ReceiptDate,
    decimal ReceiptAmount,
    string TransactionDescription,
    DateTime TransactionDate,
    decimal TransactionAmount,
    string BankStatmentFileName);
