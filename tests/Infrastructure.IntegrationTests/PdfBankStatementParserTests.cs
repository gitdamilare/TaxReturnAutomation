using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using TaxReturnAutomation.Application.Common.Interfaces;
using TaxReturnAutomation.Infrastructure.Parsing;

namespace Infrastructure.IntegrationTests;
[TestFixture]
public class PdfBankStatementParserTests
{
    private readonly string _sampleFilePath = Path.Combine("Assets", "BankStatementSample_1.pdf");
    private readonly string _sampleFilePath2 = Path.Combine("Assets", "BankStatementSample_2.pdf");

    [Test]
    public void ExtractTransactions_ShouldParseBankTransactions_FromPdf()
    {
        // Arrange
        var parser = new PdfBankStatementParser(
            Substitute.For<IFileStorageService>(),
            NullLogger<PdfBankStatementParser>.Instance);

        // Act
        var result = parser.ExtractTransactions(_sampleFilePath);

        // Assert
        result.Should().NotBeNull();
        result.Transactions.Should().NotBeNullOrEmpty();
        result.Transactions.Should().HaveCount(5);
        result.Transactions.Should().AllSatisfy(transaction =>
        {
            transaction.TransactionDate.Should().NotBe(default);
            transaction.Description.Should().NotBeNullOrWhiteSpace();
            transaction.TransactionType.Should().BeDefined();
            transaction.Amount.Should().BePositive();
        });

        foreach (var transaction in result.Transactions)
        {
            TestContext.WriteLine($"" +
                $"{transaction.TransactionDate:d} | " +
                $"{transaction.Description} | " +
                $"{transaction.TransactionType.ToString()} | " +
                $"{transaction.Amount}");
        }
    }

    [Test]
    public void ExtractTransactions_ShouldParseBankTransactions_FromPdf2()
    {
        // Arrange
        var parser = new PdfBankStatementParser(
            Substitute.For<IFileStorageService>(),
            NullLogger<PdfBankStatementParser>.Instance);

        // Act
        var result = parser.ExtractTransactions(_sampleFilePath2);

        // Assert
        result.Should().NotBeNull();
        result.Transactions.Should().NotBeNullOrEmpty();
        result.Transactions.Should().HaveCount(10);
        result.Transactions.Should().AllSatisfy(transaction =>
        {
            transaction.TransactionDate.Should().NotBe(default);
            transaction.Description.Should().NotBeNullOrWhiteSpace();
            transaction.TransactionType.Should().BeDefined();
            transaction.Amount.Should().BePositive();
        });

        foreach (var transaction in result.Transactions)
        {
            TestContext.WriteLine($"" +
                $"{transaction.TransactionDate:d} | " +
                $"{transaction.Description} | " +
                $"{transaction.TransactionType.ToString()} | " +
                $"{transaction.Amount}");
        }
    }
}
