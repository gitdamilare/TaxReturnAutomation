namespace TaxReturnAutomation.Domain.Exceptions;
internal class UnsupportedTransactionTypeException : Exception
{
    public UnsupportedTransactionTypeException(string? transactionType) 
        : base($"Transaction Type \"{transactionType}\" is unsupported.")
    {
    }
}
