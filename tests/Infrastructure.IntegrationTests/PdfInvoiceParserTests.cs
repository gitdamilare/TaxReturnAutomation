using System.Globalization;
using Azure;
using Azure.AI.DocumentIntelligence;
using Infrastructure.IntegrationTests.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using TaxReturnAutomation.Application.Common.Interfaces;
using TaxReturnAutomation.Infrastructure.Parsing;

namespace Infrastructure.IntegrationTests;
[TestFixture]
internal class PdfInvoiceParserTests
{
    private IFileStorageService _fileStorageService;
    private IInvoiceParser _parser;
    private IConfiguration _configuration;
    private IAnalyzeResultCache _analyseResultCache;

    private static Uri BlobUri(string fileName) => new($"http://127.0.0.1:10000/devstoreaccount1/invoices/{fileName}");
    private static string GetFilePath(string fileName) => Path.Combine("Assets/Invoices", fileName);

    //TODO: Update the test case file before pushing to git
    private static readonly object[] ValidTestCaseSource =
    [
        new object[] { "Rechnung08-2023-Eugeniu.pdf", "08-2023", "2400.00", string.Empty, "21.11.2023" },
        new object[] { "Invoice_130336720_XEE0YL.pdf", "130336720", "1582.44", "143512", "24.11.2023" }
    ];

    [SetUp]
    public void Setup()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddUserSecrets<PdfInvoiceParserTests>()
            .AddEnvironmentVariables();
        _configuration = configBuilder.Build();

        var key = _configuration["AzureAI:DocumentIntelligence:Key"]!;
        var endpoint = _configuration["AzureAI:DocumentIntelligence:Endpoint"]!;
        var client = new DocumentIntelligenceClient(
            new Uri(endpoint),
            new AzureKeyCredential(key));
        _fileStorageService = Substitute.For<IFileStorageService>();

        _parser = new AzureFormRecognizerInvoiceParser(
            NullLogger<AzureFormRecognizerInvoiceParser>.Instance,
            _fileStorageService,
            client,
            _analyseResultCache);
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _analyseResultCache = new JsonBasedAnalyzeResultCache();
    }

    [Test, TestCaseSource(nameof(ValidTestCaseSource))]
    public async Task ParseAsync_ShouldReturnInvoiceDto_WhenFileIsValid(
        string fileName,
        string invoiceId,
        string totalAmount,
        string customerId,
        string date)
    {
        var fileBytes = File.ReadAllBytes(GetFilePath(fileName));
        _fileStorageService.DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>())
            .Returns(fileBytes);

        var result = await _parser.ParseAsync(BlobUri(fileName), CancellationToken.None);

        result.ShouldNotBeNull();
        result.FileName.ShouldBe(fileName);
        result.InvoiceNumber.ShouldBe(invoiceId);
        result.TotalAmount.ShouldBe(decimal.Parse(totalAmount, NumberStyles.Currency, CultureInfo.InvariantCulture));
        result.CustomerId.ShouldBe(customerId);
        result.PurchaseDate.ShouldBe(date == "" ? DateTime.MinValue : DateTime.Parse(date));
    }
}
