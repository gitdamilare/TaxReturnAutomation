using TaxReturnAutomation.Application.Common.Mapper;

namespace TaxReturnAutomation.Application.Common.UseCases.BankStatements;
internal class BankStatementProcessor : IBankStatementProcessor
{
    private readonly IBankStatementRepository _bankStatementRepository;
    private readonly IBankStatementParser _bankStatementParser;

    public BankStatementProcessor(
        IBankStatementRepository bankStatementStorage,
        IBankStatementParser bankStatementParser)
    {
        _bankStatementRepository = bankStatementStorage;
        _bankStatementParser = bankStatementParser;
    }

    public async Task<ProcessBankStatementResponse> ProcessAsync(
        ProcessBankStatementRequest request, 
        CancellationToken cancellationToken)
    {
        var bankStatementDto = await _bankStatementParser.ParseAsync(
            request.FileUri,
            request.ContentType,
            cancellationToken);

        var bankStatement = BankStatementMapper.MapFromDto(bankStatementDto);

        await _bankStatementRepository.SaveBankStatementAsync(bankStatement, cancellationToken);

        return new ProcessBankStatementResponse(bankStatement.Id, bankStatement.Transactions.Count);
    }
}
