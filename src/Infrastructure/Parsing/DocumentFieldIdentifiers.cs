namespace TaxReturnAutomation.Infrastructure.Parsing;
public class DocumentFieldIdentifiers
{
    // Bank statement fields
    public const string AccountName = "AccountName";
    public const string Iban = "IBAN";
    public const string BankStatementDate = "BankStatementMonth";
    public const string BankStatementPeriod = "BankStatementMonthRange";
    public const string AverageBalance = "AverageAmount";

    // Transaction table fields
    public static class Transactions
    {
        public const string TableName = "transactions";
        public const string BookingDate = "BookingDate";
        public const string ValueDate = "ValueDate";
        public const string Description = "Description";
        public const string Credit = "Credit";
        public const string Debit = "Debit";
    }

    // Invoice fields
    public const string InvoiceNumber = "InvoiceId";
    public const string CustomerId = "CustomerId";
    public const string CustomerName = "CustomerName";
    public const string PurchaseDate = "InvoiceDate";
    public const string SubTotal = "SubTotal";
    public const string TotalTax = "TotalTax";
    public const string TotalAmount = "InvoiceTotal";
    public const string InvoiceDate = "InvoiceDate";
    public const string ItemsTable = "Items";


}
