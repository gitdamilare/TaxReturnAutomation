using Azure;
using Azure.AI.DocumentIntelligence;
using TaxReturnAutomation.Application.Common.DTOs;

namespace TaxReturnAutomation.Infrastructure.Parsing;
public class AzureFormRecognizerBankStatementParser : IBankStatementParser
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AzureFormRecognizerBankStatementParser> _logger;

    public AzureFormRecognizerBankStatementParser(
        IFileStorageService fileStorageService,
        ILogger<AzureFormRecognizerBankStatementParser> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }


    public async Task<BankStatementDto> ParseAsync(Uri fileUri, string contentType, CancellationToken cancellationToken)
    {
        var fileData = await _fileStorageService.DownloadFileAsync(fileUri, cancellationToken);
        ValidateFileData(fileData);

        var document = await AnalyzeDocument(fileData);
        var bankStatementDto = ProcessAnalyzeResult(document, ExtractFileNameFromUri(fileUri));

        if(bankStatementDto == null)
        {
            _logger.LogInformation("No bank statement data found in the document.");
            throw new ArgumentException("No bank statement data found in the document.");
        }

        return bankStatementDto;
    }

    private async Task<AnalyzeResult> AnalyzeDocument(byte[] fileData)
    {
        try
        {
            //TODO: Move to configuration
            string endpoint = "<add-endpoint>";
            string key = "<add-key>";

            AzureKeyCredential credential = new AzureKeyCredential(key);
            DocumentIntelligenceClient client = new DocumentIntelligenceClient(
                new Uri(endpoint),
                credential
                );

            string modelId = "deutsche_bank_statement";
            var binaryData = new BinaryData(fileData);
            var analyzeDocumentOptions = new AnalyzeDocumentOptions(modelId, binaryData)
            {
                Locale = "de-DE",
            };

            Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, analyzeDocumentOptions);
            AnalyzeResult result = operation.Value;

            _logger.LogInformation("Analyzed document with model: {ModelId}", result.ModelId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing document");
            throw;
        }
    }

    private BankStatementDto? ProcessAnalyzeResult(AnalyzeResult result, string documentName)
    {
        var document = result.Documents.Count > 0 ? result.Documents[0] : default;

        if (document == default)
        {
            _logger.LogWarning("No documents found in the analysis result.");
            return null;
        }

        _logger.LogInformation("Document of type: {DocumentType}", document.DocumentType);
        var bankStatementDto = ProcessDocumentFields(document, documentName);
        if (bankStatementDto == null)
        {
            _logger.LogWarning("No bank statement data found in the document.");
            throw new ArgumentException("No bank statement data found in the document.");
        }

        var transactions = ProcessTransactionFields(document);
        ValidateTransactions(transactions);
        bankStatementDto.Transactions = transactions;
        return bankStatementDto;
    }

    private BankStatementDto ProcessDocumentFields(AnalyzedDocument document, string documentName)
    {
        var bankStatementDto = new BankStatementDto(documentName);
        foreach (KeyValuePair<string, DocumentField> fieldKvp in document.Fields)
        {
            ProcessField(fieldKvp.Key, fieldKvp.Value, bankStatementDto);
        }

        return bankStatementDto;
    }

    private static readonly Dictionary<string, Action<DocumentField, BankStatementDto>> FieldHandlers = new()
    {
        [DocumentFieldIdentifiers.AccountName] = (field, bankStatementDto) => bankStatementDto.AccountHolderName = field.Content,
        [DocumentFieldIdentifiers.Iban] = (field, bankStatementDto) => bankStatementDto.AccountNumber = SanitizeIban(field.Content),
        [DocumentFieldIdentifiers.BankStatementPeriod] = (field, bankStatementDto) => bankStatementDto.StatementPeriod = field.Content,
        [DocumentFieldIdentifiers.BankStatementDate] = (field, bankStatementDto) => bankStatementDto.StatementDate = field.Content,
        [DocumentFieldIdentifiers.AverageBalance] = (field, bankStatementDto) => bankStatementDto.AverageBalance = field.Content
    };

    private void ProcessField(
        string fieldName,
        DocumentField field,
        BankStatementDto bankStatmentDto)
    {
        if (FieldHandlers.TryGetValue(fieldName, out var handler))
        {
            handler(field, bankStatmentDto);
        }
        else
        {
            _logger.LogWarning("No handler for field '{FieldName}'", fieldName);
        }
    }

    private List<BankTransactionDto> ProcessTransactionFields(AnalyzedDocument document)
    {
        if (!document.Fields.TryGetValue(DocumentFieldIdentifiers.Transactions.TableName, out var transactionsField) || 
            transactionsField.FieldType != DocumentFieldType.List)
        {
            _logger.LogWarning("No transactions field found in the document.");
            return [];
        }

        return [.. transactionsField.ValueList
            .Select(ProcessTransactionField)
            .Where(transactionField => transactionField != null)];
    }

    private BankTransactionDto? ProcessTransactionField(DocumentField transactionField)
    {
        if (transactionField.FieldType != DocumentFieldType.Dictionary)
        {
            _logger.LogWarning("Transaction field is not a dictionary.");
            return default;
        }

        var transactionData = transactionField.ValueDictionary;

        var bookingDate = ParseDate(GetFieldValue(transactionData, DocumentFieldIdentifiers.Transactions.BookingDate));
        var valueDate = ParseDate(GetFieldValue(transactionData, DocumentFieldIdentifiers.Transactions.ValueDate));
        var description = GetFieldValue(transactionData, DocumentFieldIdentifiers.Transactions.Description);
        var (amount, type) = ParseAmount(
            GetFieldValue(transactionData, DocumentFieldIdentifiers.Transactions.Credit),
            GetFieldValue(transactionData, DocumentFieldIdentifiers.Transactions.Debit));

        if (!bookingDate.HasValue || !valueDate.HasValue || !amount.HasValue)
        {
            _logger.LogWarning("Invalid transaction data");
            return null;
        }

        return new BankTransactionDto(
            BookingDate: bookingDate.Value,
            ValueDate: valueDate.Value,
            Description: description ?? string.Empty,
            Amount: amount.Value,
            type: type!.Value);
    }

    private static string? GetFieldValue(
        IReadOnlyDictionary<string, DocumentField> fields,
        string key)
    {
        return fields.TryGetValue(key, out var field) && field.FieldType == DocumentFieldType.String
            ? field.Content?.Trim()
            : null;
    }

    private static DateTime? ParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString)) return null;

        var cleanedDate = dateString
            .Replace("\n", "")
            .Replace(" ", "");

        return DateTime.TryParseExact(cleanedDate, "dd.MM.yyyy",
            CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out var date)
            ? date
            : default;
    }

    private static string SanitizeIban(string iban) => iban?.Replace(" ", "") ?? "";

    private static (decimal? Amount, TransactionType? TransactionType) ParseAmount(string? credit, string? debit)
    {

        var (amountStr, type) = GetAmountInfo(credit, debit);
        if (string.IsNullOrWhiteSpace(amountStr)) return (null, null);

        var sanitizedAmount = amountStr
            .Replace(".", "")  // remove thousands separator
            .Replace(",", ".") //convert decimal separator
            .Replace(" ", "")
            .Replace("+", "")
            .Replace("-", "")
            .Trim();

        if (!decimal.TryParse(
            sanitizedAmount,
            NumberStyles.Currency,
            CultureInfo.InvariantCulture, 
            out decimal amount))
        {
            return (null, null);
        }

        return (Math.Abs(amount), type);
    }

    private static (string? Amount, TransactionType? Type) GetAmountInfo(string? credit, string? debit)
    {
        if (!string.IsNullOrWhiteSpace(credit))
            return (credit, TransactionType.Credit);

        if (!string.IsNullOrWhiteSpace(debit))
            return (debit, TransactionType.Debit);

        return (null, null);
    }

    private static string ExtractFileNameFromUri(Uri fileUri) => Path.GetFileName(fileUri.LocalPath);

    private void ValidateFileData(byte[] fileData)
    {
        if (fileData?.Length == 0)
        {
            _logger.LogInformation("File data is empty");
            throw new ArgumentException("Invalid file data");
        }
    }

    private void ValidateTransactions(List<BankTransactionDto>? transactions)
    {
        if (transactions == null || transactions.Count == 0)
        {
            _logger.LogInformation("No transactions found");
            throw new ArgumentException("No transactions found");
        }
    }
}
