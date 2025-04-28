namespace TaxReturnAutomation.Application.Common.UseCases.BankStatements;
internal class BankStatementProcessor : IBankStatementProcessor
{
    private readonly IBankStatementStorage _bankStatementStorage;
    private readonly IBankStatementParser _bankStatementParser;

    public BankStatementProcessor(
        IBankStatementStorage bankStatementStorage,
        IBankStatementParser bankStatementParser)
    {
        _bankStatementStorage = bankStatementStorage;
        _bankStatementParser = bankStatementParser;
    }

    public async Task<ProcessBankStatementResponse> ProcessAsync(
        ProcessBankStatementRequest request, 
        CancellationToken cancellationToken)
    {
        var bankStatement = await _bankStatementParser.ParseAsync(
            request.BlobUri,
            request.ContentType,
            cancellationToken);

        await _bankStatementStorage.SaveBankStatementAsync(bankStatement, cancellationToken);

        return new ProcessBankStatementResponse(bankStatement.Id, bankStatement.Transactions.Count);
    }
}
