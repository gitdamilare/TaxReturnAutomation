namespace TaxReturnAutomation.Infrastructure.Parsing;
public class PdfBankStatementParser : IBankStatementParser
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<PdfBankStatementParser> _logger;

    public PdfBankStatementParser(
        IFileStorageService fileStorageService, 
        ILogger<PdfBankStatementParser> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<BankStatement> ParseAsync(
        string blobUri,
        string contentType,
        CancellationToken cancellationToken)
    {
        var fileData = await _fileStorageService.DownloadFileAsync(blobUri, cancellationToken);

        if (fileData == null || fileData.Length == 0)
        {
            _logger.LogInformation("File data is empty or null.");
            throw new ArgumentException("File data is empty or null.");
        }

        var fileName = ExtractFileNameFromUri(blobUri);

        return string.IsNullOrEmpty(fileName)
            ? throw new ArgumentException("File name is empty or null.")
            : ExtractBankStatement(fileData, fileName);
    }

    private static BankStatement ExtractBankStatement(byte[] fileBytes, string fileName)
    {
        var transactionRegex = new Regex(
            @"(\d{2}\.\d{2}\.)\s+(\d{2}\.\d{2}\.)\s+(.*?)\s+([+-]?\s?\d[\d\., ]+)\s+(\d{4})\s+(\d{4})\s+(.*?)(?=\s+\d{2}\.\d{2}\.|\s*$)",
            RegexOptions.Singleline
        );

        var bankStatment = BankStatement.Create(fileName);
        using (var stream = new MemoryStream(fileBytes))
        using (var pdf = PdfDocument.Open(stream))
        {
            foreach (var page in pdf.GetPages())
            {
                string pageText = page.Text;
                var matches = transactionRegex.Matches(pageText);

                foreach (Match match in matches)
                {
                    var bookingDateStr = match.Groups[1].Value;
                    var valueDateStr = match.Groups[2].Value;
                    var descriptionStr = match.Groups[3].Value;
                    var amountStr = match.Groups[4].Value;
                    var bookingYearStr = match.Groups[5].Value;
                    var valueYearStr = match.Groups[6].Value;
                    var additionalDescriptionStr = match.Groups[7].Value;

                    var transactionDate = ParseTransactionDate(
                        bookingDateStr, bookingYearStr, valueDateStr, valueYearStr);

                    var (amount, transactionType) = ParseAmount(amountStr);


                    var fullDescription = ParseTransactionDescription(descriptionStr, additionalDescriptionStr);

                    // Create transaction
                    var transaction = BankTransaction.Create(
                        transactionDate,
                        amount,
                        fullDescription,
                        transactionType,
                        bankStatment.Id
                    );

                    bankStatment.AddTransaction(transaction);
                }
            }
        }

        return bankStatment;
    }

    private static string ExtractFileNameFromUri(string blobUri)
    {
        return Path.GetFileName(new Uri(blobUri).LocalPath);
    }

    private static (decimal amount, TransactionType transactionType) ParseAmount(string amountStr)
    {
        var cleanAmount = amountStr
            .Replace(" ", "")
            .Replace(",", ".")
            .Trim();

        var isCredit = cleanAmount.StartsWith("+");
        var numericValue = cleanAmount.TrimStart('+', '-');

        if (decimal.TryParse(numericValue, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal amount))
        {
            var transactionType = isCredit ? TransactionType.Credit : TransactionType.Debit;
            return (Math.Abs(amount), transactionType);
        }

        throw new FormatException($"Invalid amount format: {amountStr}");
    }

    private static DateTime ParseTransactionDate(
        string bookingDate, 
        string bookingYear, 
        string valueDate, 
        string valueYear)
    {
        return DateTime.ParseExact(
            $"{bookingDate.Trim()}{bookingYear.Trim()}",
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture);
    }

    private static string ParseTransactionDescription(string descriptionStr, string additionalDescriptionStr)
    {
        var cleanupRegex = new Regex(@"\s*\d{9,10}\s*/\s*\d{8}\s*/\s*\d{8}\s*$");

        var cleanMainDesc = Regex.Replace(descriptionStr, @"\s+", " ").Trim();
        var cleanAddDesc = Regex.Replace(additionalDescriptionStr, @"\s+", " ").Trim();

        var fullDescription = cleanupRegex.Replace($"{cleanMainDesc} {cleanAddDesc}".Trim(), "");
        return Regex.Replace(fullDescription, @"\s+", " ").Trim();
    }
}
