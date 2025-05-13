using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TaxReturnAutomation.Application.Common.Interfaces;
using TaxReturnAutomation.Infrastructure.Parsing;

namespace Infrastructure.IntegrationTests;
[TestFixture]
public class PdfBankStatementParserTests
{
    private IFileStorageService _fileStorageService;
    private PdfBankStatementParser _parser;

    [SetUp]
    public void Setup()
    {
        _fileStorageService = Substitute.For<IFileStorageService>();
        _parser = new PdfBankStatementParser(
            _fileStorageService,
            NullLogger<PdfBankStatementParser>.Instance);
    }

    private static readonly object[] TestFiles =
    [
        new object[] { "BankStatementSample_1.pdf", 5 },
        new object[] { "BankStatementSample_2.pdf", 10 }
    ];
    private static Uri BlobUri(string fileName) => new($"https://storage/account/container/{fileName}");

    [Test, TestCaseSource(nameof(TestFiles))]
    public async Task ParseAsync_ShouldReturnBankStatement_WhenFileIsValid(
        string fileName,
        int expectedTransactionCount)
    {
        var filePath = Path.Combine("Assets", fileName);
        var fileBytes = File.ReadAllBytes(filePath);
        _fileStorageService.DownloadFileAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>())
            .Returns(fileBytes);
       
        var result = await _parser.ParseAsync(BlobUri(fileName), "application/pdf", CancellationToken.None);

        result.Should().NotBeNull();
        result.FileName.Should().Be(fileName);
        result.Transactions.Should().NotBeNullOrEmpty();
        result.Transactions.Should().HaveCount(expectedTransactionCount);
    }
}
