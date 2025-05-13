namespace TaxReturnAutomation.Application.Common.Mapper;
public class BankStatementMapper
{
    public static BankStatement MapFromDto(
       BankStatementDto bankStatementDto)
    {
        var bankStatement = BankStatement.Create(bankStatementDto.FileName);
        foreach (var transactionDto in bankStatementDto.Transactions)
        {
            if (TryParseTransaction(transactionDto, bankStatement.Id, out var transaction))
            {
                bankStatement.AddTransaction(transaction!);
            }
        }
        return bankStatement;
    }

    //TODO: Implement a proper parser for the transaction
    private static bool TryParseTransaction(
        BankTransactionDto dto, 
        Guid statementId, 
        out BankTransaction? transaction)
    {
        transaction = null;

        //if (!DateTime.TryParse(dto.BookingDate, out var date)) return false;

        //var amountStr = !string.IsNullOrWhiteSpace(dto.Credit) ? dto.Credit : dto.Debit;
        //var isCredit = !string.IsNullOrWhiteSpace(dto.Credit);

        //if (!decimal.TryParse(amountStr?.Replace("+", "").Replace("-", "").Trim(), NumberStyles.Any, CultureInfo.GetCultureInfo("de-DE"), out var amount))
        //    return false;

        ////amount = isCredit ? amount : -amount;
        //var type = isCredit ? TransactionType.Credit : TransactionType.Debit;

        transaction = BankTransaction.Create(
            dto.BookingDate,
            dto.Amount,
            dto.Description,
            dto.type,
            statementId);
        return true;
    }
}
