using Azure;
using Azure.AI.DocumentIntelligence;
using TaxReturnAutomation.Application.Common.DTOs;
using TaxReturnAutomation.Infrastructure.Extensions;

namespace TaxReturnAutomation.Infrastructure.Parsing;
public class AzureFormRecongnizerInvoiceParser : IInvoiceParser
{
    private readonly ILogger<AzureFormRecongnizerInvoiceParser> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly DocumentIntelligenceClient _documentIntelligenceClient;
    private readonly IAnalyzeResultCache _analyzeResultCache;

    public AzureFormRecongnizerInvoiceParser(
        ILogger<AzureFormRecongnizerInvoiceParser> logger,
        IFileStorageService fileStorageService,
        DocumentIntelligenceClient documentIntelligenceClient,
        IAnalyzeResultCache analyzeResultCache)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
        _documentIntelligenceClient = documentIntelligenceClient;
        _analyzeResultCache = analyzeResultCache;
    }

    //TODO: Clean up the code and remove unused methods
    public async Task<InvoiceDto> ParseAsync(Uri fileUri, CancellationToken cancellationToken)
    {
        //Download the file
        var fileData = await _fileStorageService.DownloadFileAsync(fileUri, cancellationToken);
        ValidateFileData(fileData);

        //Analyze the document
        var analyzeResult = await AnalyzeDocument(fileData, fileUri.GetFileName());

        //Process the analyze result
        var invoiceDtos = ProcessAnalyzeResult(analyzeResult, fileUri.GetFileName());

        //TODO: support multiple invoices
        return invoiceDtos?.FirstOrDefault() ?? new InvoiceDto("");
    }

    private async Task<AnalyzeResult> AnalyzeDocument(byte[] fileData, string fileName)
    {
        try
        {
            const string modelId = "prebuilt-invoice";
            var analyzeDocumentOptions = new AnalyzeDocumentOptions(modelId, new BinaryData(fileData))
            {
                Locale = "de-DE",
            };

            var cachedAnalyzeResult = await _analyzeResultCache.GetAsync(fileData, modelId);

            if(cachedAnalyzeResult != null)
            {
                _logger.LogInformation("Using cached analyze result for model: {ModelId}", modelId);
                return cachedAnalyzeResult;
            }

            AnalyzeResult result = (await _documentIntelligenceClient.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                analyzeDocumentOptions)).Value;

            await _analyzeResultCache.SetAsync(fileData, modelId, result, TimeSpan.FromDays(30));

            _logger.LogInformation("Analyzed document with model: {ModelId}", result.ModelId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing document");
            throw;
        }
    }

    private List<InvoiceDto> ProcessAnalyzeResult(AnalyzeResult analyzeResult, string fileName)
    {
        var invoiceDtos = new List<InvoiceDto>();
        foreach (var document in analyzeResult.Documents)
        {
            var invoiceDto = ProcessDocumentFields(document, fileName);

            UpdateMissingInvoiceContent(invoiceDto, document);
            invoiceDtos.Add(invoiceDto);
        }

        return invoiceDtos;
    }

    private static void UpdateMissingInvoiceContent(InvoiceDto invoiceDto, AnalyzedDocument document)
    {
        if(invoiceDto.TotalAmount == 0)
        {
            invoiceDto.TotalAmount = ProcessTotalInvoiceAmountFromItems(document);
        }
    }

    private InvoiceDto ProcessDocumentFields(AnalyzedDocument document, string fileName)
    {
        var invoiceDto = new InvoiceDto(fileName);

        foreach (var field in document.Fields)
        {
            var fieldName = field.Key;
            var fieldValue = field.Value;

            // Process the field value as needed
            if(_fieldMappings.TryGetValue(fieldName, out var action))
            {
                action(fieldValue, invoiceDto);
            }

            _logger.LogInformation("Field: {FieldName}, Value: {FieldValue}", fieldName, fieldValue);
        }

        return invoiceDto;
    }

    private readonly Dictionary<string, Action<DocumentField, InvoiceDto>> _fieldMappings = new ()
    {
        { DocumentFieldIdentifiers.InvoiceNumber, (field, dto) => dto.InvoiceNumber = field.Content },
        { DocumentFieldIdentifiers.InvoiceDate, (field, dto) => dto.PurchaseDate = DateTime.Parse(field.Content) },
        { DocumentFieldIdentifiers.CustomerName, (field, dto) => dto.CustomerName = field.Content.ToString() },
        { DocumentFieldIdentifiers.TotalAmount, (field, dto) => dto.TotalAmount = decimal.Parse(field.Content.ToString() ?? "0", NumberStyles.Currency, CultureInfo.InvariantCulture) },
        { DocumentFieldIdentifiers.TotalTax, (field, dto) => dto.TotalTax = decimal.Parse(field.Content.ToString() ?? "0", NumberStyles.Currency, CultureInfo.InvariantCulture) },
        { DocumentFieldIdentifiers.SubTotal, (field, dto) => dto.SubTotal = decimal.Parse(field.Content.ToString() ?? "0", NumberStyles.Currency, CultureInfo.InvariantCulture) },
    };

    private static decimal ProcessTotalInvoiceAmountFromItems(AnalyzedDocument document)
    {
        if (!document.Fields.TryGetValue(DocumentFieldIdentifiers.ItemsTable, out var itemsField) ||
            itemsField.FieldType != DocumentFieldType.List)
        {
            //_logger.LogWarning("No items field found in the invoice document.");
            return 0;
        }

        double? totalAmount = 0;
        foreach (var documentField in itemsField.ValueList)
        {
            documentField.ValueDictionary.TryGetValue("Amount", out var amountField);

            if (amountField?.FieldType == DocumentFieldType.Currency)
            {
                var amount = amountField?.ValueCurrency.Amount;
                totalAmount += amount;
            }
        }

        return decimal.Parse(totalAmount.ToString() ?? "0", NumberStyles.Currency, CultureInfo.InvariantCulture);
    }

    private void ValidateFileData(byte[] fileData)
    {
        if (fileData?.Length == 0)
        {
            _logger.LogInformation("File data is empty");
            throw new ArgumentException("Invalid file data");
        }
    }
}
