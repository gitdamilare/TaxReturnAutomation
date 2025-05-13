using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.IdentityModel.Tokens;
using TaxReturnAutomation.Application.Common.DTOs;

namespace TaxReturnAutomation.Infrastructure.Parsing;

//TODO: remove this class and use the new one
public class AzureFormRecognizerBankStatementParserOld : IBankStatementParser
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PdfBankStatementParser> _logger;

    public AzureFormRecognizerBankStatementParserOld(
        IFileStorageService fileStorageService,
        ILogger<PdfBankStatementParser> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }


    public async Task<BankStatement> ParseAsync(Uri fileUri, string contentType, CancellationToken cancellationToken)
    {
        var fileData = await _fileStorageService.DownloadFileAsync(fileUri, cancellationToken);

        if (fileData == null || fileData.Length == 0)
        {
            _logger.LogInformation("File data is empty or null.");
            throw new ArgumentException("File data is empty or null.");
        }

        var fileName = ExtractFileNameFromUri(fileUri);

        var bankStatement = BankStatement.Create(fileName);
        var transactions = await ExtractBankTransactionData(fileData, fileName, bankStatement.Id);
        if (transactions == null || transactions.Count == 0)
        {
            _logger.LogInformation("No transactions found in the file.");
            throw new ArgumentException("No transactions found in the file.");
        }

        foreach (var transaction in transactions)
        {
            bankStatement.AddTransaction(transaction);
        }

        return bankStatement;
    }

    private async Task<List<BankTransaction>> ExtractBankTransactionData(
        byte[] fileData,
        string fileName,
        Guid bankStatementId)
    {
        //TODO: Move to configuration
        string endpoint = "<add-endpoint>";
        string key = "<add-key>";
        AzureKeyCredential credential = new AzureKeyCredential(key);
        DocumentIntelligenceClient client = new DocumentIntelligenceClient(new Uri(endpoint), credential);

        var binaryData = new BinaryData(fileData);
        Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", binaryData);
        AnalyzeResult result = operation.Value;

        ConsoleTable(result);
        var transactions = new List<BankTransaction>();

        for (int i = 1; i < result.Tables.Count; i++)
        {
            DocumentTable table = result.Tables[i];

            const int headerCellCount = 5;
            var headerCells = table.Cells.Where(c => c.RowIndex == 0).ToList();

            if (headerCells.Count >= headerCellCount)
            {
                ParseTransactionTable(table, transactions, bankStatementId, "");
                continue;
            }
        }

        return transactions;
    }

    private void ConsoleTable(AnalyzeResult result)
    {
        Console.WriteLine("The following tables were extracted:");

        for (int i = 0; i < result.Tables.Count; i++)
        {
            DocumentTable table = result.Tables[i];
            Console.WriteLine($"  Table {i} has {table.RowCount} rows and {table.ColumnCount} columns.");

            foreach (DocumentTableCell cell in table.Cells)
            {
                Console.WriteLine($"    Cell ({cell.RowIndex}, {cell.ColumnIndex}) has kind '{cell.Kind}' and content: '{cell.Content}'.");
            }
        }
    }

    private void ParseTransactionTable(
    DocumentTable table,
    List<BankTransaction> transactions,
    Guid bankStatementId,
    string newString ="")
    {
        var cellsByRow = table.Cells
                          .GroupBy(c => c.RowIndex)
                          .ToDictionary(
                              g => g.Key,
                              g => g.OrderBy(c => c.ColumnIndex).ToList()
                          );

        string currentTransactionYear = DateTime.MinValue.Year.ToString();

        // Rows 0 is header, so start at 1, and advance by 2 rows per transaction
        for (int r = 1; r < table.RowCount; r += 2)
        {
            // fetch the two rows
            var row1 = cellsByRow[r];     // date+amount row
            var row2 = cellsByRow[r + 1]; // description (and maybe year)

            // 1) build the date
            var dayMonth = row1[0].Content.Trim(); // e.g. "11.09."
            var year = row2[0].Content.Trim() ; // e.g. "2023"
            if(string.IsNullOrWhiteSpace(year))
            {
                year = currentTransactionYear;
            }
            else
            {
                currentTransactionYear = year;
            }
            var dateStr = dayMonth + year;       // "11.09.2023"
            var date = DateTime.ParseExact(dateStr, "dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"));

            // 2) pick the amount cell
            //    if col‐3 (Soll) empty, use col‐4 (Haben), else use col‐3
            var rawAmountCell = !string.IsNullOrWhiteSpace(row1[3].Content)
                                  ? row1[3]
                                  : row1[4];

            var rawAmt = rawAmountCell.Content.Trim(); // e.g. "+ 5.000,00" or "- 790,50"
            bool isCredit = rawAmt.StartsWith("+");
            // normalize German number to invariant: remove sign, remove thousands “.”, switch “,”→“.”
            var normalized = rawAmt
                .Replace("+", "")
                .Replace("-", "")
                .Replace(".", "")
                .Replace(",", ".");
            var amount = decimal.Parse(normalized, CultureInfo.InvariantCulture);

            // 3) build the description
            //    row2[2] is the first description line; if there's a third line (r+2) and it's text, append it
            var description = row1[2].Content.Trim();
            if (cellsByRow.TryGetValue(r + 1, out var maybeRow2))
            {
                var extra = maybeRow2[2].Content.Trim();
                if (!string.IsNullOrWhiteSpace(extra))
                    description += " " + extra;
            }

            transactions.Add(BankTransaction.Create(
            transactionDate: date,
            amount: amount,
            description: description,
            transactionType: TransactionType.Credit,
            bankStatementId: bankStatementId));
        }
    }

    private void ParseTransactionTable(
        DocumentTable table,
        List<BankTransaction> transactions,
        Guid bankStatementId)
    {
        var cellsByRow = table.Cells
                       .GroupBy(c => c.RowIndex)
                       .ToDictionary(
                         g => g.Key,
                         g => g.OrderBy(c => c.ColumnIndex).ToList());

        for (int rowIndex = 1; rowIndex < table.RowCount; rowIndex += 2)
        {
            if(rowIndex + 1 >= table.RowCount)
                break;

            try
            {
                var rowPair1 = table.Cells.Where(c => c.RowIndex == rowIndex).ToList();
                var rowPair2 = table.Cells.Where(c => c.RowIndex == rowIndex + 1).ToList();
                var transaction = ParseTransactionRows(rowPair1, rowPair2, bankStatementId);
                transactions.Add(transaction);
            }
            catch (Exception)
            {
                _logger.LogError($"Failed to parse transaction at row {rowIndex}.");
                throw;
            }
        }
    }

    private BankTransaction ParseTransactionRows(
        List<DocumentTableCell> rowPair1,
        List<DocumentTableCell> rowPair2,
        Guid bankStatmentId)
    {
        string rawdate = GetCellContent(rowPair1, 0); // day.month
        string additionalDate = GetCellContent(rowPair2, 1); //year 
        var parsedDate = GetParsedDate(rawdate, additionalDate);

        var parsedDescription = GetParsedDescription(
            description: GetCellContent(rowPair1, 2),
            additionalDescription: GetCellContent(rowPair2, 2));

        string debitAmount = GetCellContent(rowPair1, 3);
        string creditAmount = GetCellContent(rowPair2, 3);
        //string rawAmount = GetCellContent(rowPair1, 3).IsNullOrEmpty() ? (string.IsNullOrEmpty(GetCellContent(rowPair1, 4)) ? GetCellContent(rowPair2, 3) : GetCellContent(rowPair2, 4)) : "";
        string rawAmount = !GetCellContent(rowPair1, 3).IsNullOrEmpty() ?
             GetCellContent(rowPair1, 3) : 
             ( !GetCellContent(rowPair1, 4).IsNullOrEmpty() ? GetCellContent(rowPair1, 4) : 
             !GetCellContent(rowPair2, 4).IsNullOrEmpty() ? GetCellContent(rowPair2, 4) : GetCellContent(rowPair2, 3));

        //bool isCreditCheck = !string.IsNullOrEmpty(creditAmount);
        //string rawAmount = isCreditCheck ? creditAmount : debitAmount;
        var (parsedAmount, transactionType) = ParseAmount(rawAmount);

        return BankTransaction.Create(
            transactionDate: parsedDate,
            amount: parsedAmount,
            description: parsedDescription,
            transactionType: transactionType,
            bankStatementId: bankStatmentId);
    }

    private static DateTime GetParsedDate(string rawDate, string additionalDate)
    {
        var combinedDate = $"{rawDate}.{additionalDate}";
        return DateTime.TryParse(combinedDate, out var transactionDate) ?
            transactionDate :
            DateTime.MinValue;
    }

    private static string GetParsedDescription(string description, string additionalDescription) => 
        $"{description} {additionalDescription}".Trim();

    private static (decimal amount, TransactionType transactionType) ParseAmount(string amountStr)
    {
        var cleanAmount = amountStr
            .Replace(" ", "")
            .Trim();

        var isCredit = cleanAmount.StartsWith("+");
        var numericValue = cleanAmount.TrimStart('+', '-');

        if (decimal.TryParse(numericValue, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("de-DE"), out decimal amount))
        {
            var transactionType = isCredit ? TransactionType.Credit : TransactionType.Debit;
            return (Math.Abs(amount), transactionType);
        }

        throw new FormatException($"Invalid amount format: {amountStr}");
    }

    private string GetCellContent(List<DocumentTableCell> cells, int columnIndex)
    {
        return cells.FirstOrDefault(c => c.ColumnIndex == columnIndex)?.Content ?? "";
    }

    //public async Task ExtractBankStatementData(byte[] fileBytes, string fileName)
    //{
    //    // sample document
    //    Uri fileUri = new Uri("https://raw.githubusercontent.com/Azure-Samples/cognitive-services-REST-api-samples/master/curl/form-recognizer/sample-layout.pdf");

    //    var binaryData = new BinaryData(fileBytes);
    //    Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", binaryData);

    //    AnalyzeResult result = operation.Value;

    //    Console.WriteLine("The following tables were extracted:");
    //    var tableCount = result.Tables.Count - 1;

    //    for (int i = 1; i < tableCount; i++)
    //    {
    //        DocumentTable table = result.Tables[i];
    //        Console.WriteLine($"  Table {i} has {table.RowCount} rows and {table.ColumnCount} columns.");

    //        var headerRow = table.Cells.Where(c => c.RowIndex == 0).ToList();

    //        foreach (DocumentTableCell cell in table.Cells)
    //        {
    //            Console.WriteLine($"   Cell ({cell.RowIndex}, {cell.ColumnIndex}) has kind '{cell.Kind}' and content: '{cell.Content}'.");
    //        }
    //    }
    //}

    private static string ExtractFileNameFromUri(Uri fileUri) => Path.GetFileName(fileUri.LocalPath);

    Task<BankStatementDto> IBankStatementParser.ParseAsync(Uri fileUri, string contentType, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
