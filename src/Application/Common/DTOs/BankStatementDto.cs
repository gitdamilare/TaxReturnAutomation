namespace TaxReturnAutomation.Application.Common.DTOs;
public class BankStatementDto
{
    public string? AccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public string? BankName { get; set; }
    public string? FileName { get; set; }
    public string? StatementPeriod { get; set; }
    public string? StatementDate { get; set; }
    public string? AverageBalance { get; set; }
    public List<BankTransactionDto> Transactions { get; set; }

    public BankStatementDto(
        string fileName)
    {
        FileName = fileName;
        Transactions = [];
    }

    public BankStatementDto(
        string accountNumber,
        string accountHolderName,
        string bankName,
        string fileName,
        string statementPeriod,
        string statementDate,
        string averageBalance,
        List<BankTransactionDto> transactions)
    {
        AccountNumber = accountNumber;
        AccountHolderName = accountHolderName;
        BankName = bankName;
        FileName = fileName;
        StatementPeriod = statementPeriod;
        StatementDate = statementDate;
        AverageBalance = averageBalance;
        Transactions = transactions;
    }
}
