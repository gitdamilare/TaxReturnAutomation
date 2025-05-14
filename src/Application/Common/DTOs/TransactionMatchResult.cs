namespace TaxReturnAutomation.Application.Common.DTOs;

//TODO: Add Properties 
public record TransactionMatchResult(
    Guid TransactionId,
    Guid InvoiceId,
    string InvoiceName,
    string InvoiceBlobUri,
    DateTime InvoiceDate,
    decimal InvoiceAmount,
    string TransactionDescription,
    DateTime TransactionDate,
    decimal TransactionAmount,
    string BankStatmentFileName);
