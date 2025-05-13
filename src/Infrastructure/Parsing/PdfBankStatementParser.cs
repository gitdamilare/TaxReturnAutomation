using TaxReturnAutomation.Application.Common.DTOs;
using UglyToad.PdfPig.Graphics;

namespace TaxReturnAutomation.Infrastructure.Parsing;

//TODO: Delete this class if not needed
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
        Uri fileUri,
        string contentType,
        CancellationToken cancellationToken)
    {
        var fileData = await _fileStorageService.DownloadFileAsync(fileUri, cancellationToken);

        if (fileData == null || fileData.Length == 0)
        {
            _logger.LogInformation("File data is empty or null.");
            throw new ArgumentException("File data is empty or null.");
        }

        var fileName = ExtractFileNameFromUri(fileUri);

        return string.IsNullOrEmpty(fileName)
            ? throw new ArgumentException("File name is empty or null.")
            : ExtractBankStatementDS(fileData, fileName);
    }

    private static BankStatement ExtractBankStatementDS(byte[] fileBytes, string fileName)
    {
        var transactionRegex1 = new Regex(
            @"(\d{2}\.\d{2}\.)\s+(\d{2}\.\d{2}\.)\s+(.*?)\s+([+-]?\s?\d[\d\., ]+)\s+(\d{4})\s+(\d{4})\s+(.*?)(?=\s+\d{2}\.\d{2}\.|\s*$)",
            RegexOptions.Singleline
        );

        var transactionRegex3 = new Regex(
            @"(\d{2}\.\d{2}\.)(\d{2}\.\d{2}\.)(.*?)([+-]?\s?\d[\d\., ]+)(\d{4})(\d{4})(.*?)(?=\s+\d{2}\.\d{2}\.|\s*$)",
            RegexOptions.Singleline);

        var transactionRegex2 = new Regex(
            @"(\d{2}\.\d{2}\.)(\d{2}\.\d{2}\.)(SEPA[\p{L}\d]*)([+-]\d+,\d{2})(\d{4})(\d{4})(.*?)(?=\d{2}\.\d{2}\.|$)",
            RegexOptions.Singleline);

        var t = new Regex(
            @"([+-]\d+,\d{2})(.*?)(\d{2}\.\d{2}\.\d{3})(\d{2}\.\d{2}\.\d{3}) \s+([+-]?\s?\d[\d\., ]+)\s+(\d{4})\s+(\d{4})\s+(.*?)(?=\s+\d{2}\.\d{2}\.|\s*$)",
            RegexOptions.Singleline
        );

        // Enhanced regex pattern for concatenated text
        var transactionRegex = new Regex(
            @"(?<Amount>[+-]\d{1,3}(?:\.\d{3})*,\d{2})" +      // Amount with . thousand separator
            @"(?<Description>.*?)" +                            // Description (non-greedy)
            @"(?<BookingDate>\d{2}\.\d{2}\.\d{4})" +           // Booking date
            @"(?<ValutaDate>\d{2}\.\d{2}\.\d{4})",              // Valuta date
            RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace
        );



        var bankStatment = BankStatement.Create(fileName);
        using (var stream = new MemoryStream(fileBytes))
        using (var pdf = PdfDocument.Open(stream))
        {
            foreach (var page in pdf.GetPages())
            {
                string pageText = page.Text;
                Console.WriteLine(pageText);
                var matches = transactionRegex.Matches(pageText);

                foreach (Match match in matches)
                {
                    var rawamount = match.Groups["Amount"].Value;
                    var rawDescription = match.Groups["Description"].Value;
                    var rawbookingDate = match.Groups["BookingDate"].Value;

                    var d = FormatDescription(rawDescription);

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

    public BankStatement ExtractBankStatement(byte[] fileBytes, string fileName)
    {
        var bankStatement = BankStatement.Create(Path.GetFileName(fileName));

        // Enhanced regex pattern with precise description boundary
        // Enhanced regex pattern with transaction type capture
        var transactionRegex = new Regex(
            @"(?<Amount>[+-]\d{1,3}(?:\.\d{3})*,\d{2})" +  // Amount
            @"(?<TransactionType>SEPAÜberweisungvon|SEPALastschrifteinzugvon)" + // Transaction type
            @"(?<Description>.*?)" +                         // Main description
            @"(?<BookingDate>\d{2}\.\d{2}\.\d{4})" +        // Booking date
            @"(?<ValutaDate>\d{2}\.\d{2}\.\d{4})",          // Valuta date
            RegexOptions.Singleline | RegexOptions.IgnoreCase
        );

        // Cleanup patterns
        // var technicalPrefixCleanup = new Regex(@"^.*?(SEPAÜberweisungvon|SEPALastschrifteinzugvon)");
        // var referenceCleanup = new Regex(@"\b(\d{9,10}/\d{8}/\d{8}|[A-Z]{3}\.\d{3}\.\w+\.\d+)\b");

        var bankStatment = BankStatement.Create(fileName);
        using (var stream = new MemoryStream(fileBytes))
        using (var pdf = PdfDocument.Open(stream))
        {
            foreach (var page in pdf.GetPages())
            {
                var pageText = page.Text;
                Console.WriteLine(pageText);
                var matches = transactionRegex.Matches(pageText);

                foreach (Match match in matches)
                {
                    // Extract components
                    var amount = match.Groups["Amount"].Value;
                    var transactionType = match.Groups["TransactionType"].Value;
                    var rawDescription = match.Groups["Description"].Value;
                    var bookingDate = match.Groups["BookingDate"].Value;

                    // Rebuild full description
                    var fullDescription = $"{transactionType}{rawDescription}";

                    // Clean and format description
                    fullDescription = Regex.Replace(fullDescription,
                        @"(\p{Ll})(\p{Lu})|(\D)(\d)|(\d)(\D)",
                        "$1 $2$3 $4$5 $6");

                    fullDescription = Regex.Replace(fullDescription,
                        @"\s+", " ").Trim();

                    // Parse amount and date
                    var amountValue = decimal.Parse(
                        amount.Replace(".", "").Replace(",", "."),
                        NumberStyles.Currency,
                        CultureInfo.InvariantCulture
                    );

                    var transactionDate = DateTime.ParseExact(
                        bookingDate,
                        "dd.MM.yyyy",
                        CultureInfo.InvariantCulture
                    );

                    bankStatement.AddTransaction(BankTransaction.Create(
                        transactionDate,
                        amountValue,
                        fullDescription,
                        amount.StartsWith("+") ? TransactionType.Credit : TransactionType.Debit,
                        bankStatement.Id
                    ));
                }
            }
        }
            return bankStatement;
    }

    private static string FormatDescription(string rawDescription)
    {
        // 1. Insert spaces between words
        var spaced = Regex.Replace(rawDescription,
            @"(\p{Ll})(\p{Lu})|(\D)(\d)|(\d)(\D)",
            "$1 $2$3 $4$5 $6");

        // 2. Remove technical patterns
        var cleaned = Regex.Replace(spaced,
            @"(\d{9,10}/\d{8}/\d{8}|[A-Z]{3}\.\d{3}\.\w+\.\d+)$",
            "");

        // 3. Final cleanup
        return Regex.Replace(cleaned, @"\s+", " ").Trim();
    }

    private static string ExtractFileNameFromUri(Uri fileUri) => Path.GetFileName(fileUri.LocalPath);

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

    Task<BankStatementDto> IBankStatementParser.ParseAsync(Uri fileUri, string contentType, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
