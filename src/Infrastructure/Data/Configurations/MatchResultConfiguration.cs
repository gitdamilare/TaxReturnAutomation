namespace TaxReturnAutomation.Infrastructure.Data.Configurations;
public class MatchResultConfiguration : IEntityTypeConfiguration<MatchResult>
{
    public void Configure(EntityTypeBuilder<MatchResult> builder)
    {
        const string TableName = "MatchResults";
        const int MatchConfidenceMaxLength = 20;

        builder.ToTable(TableName);

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MatchedAmount)
            .IsRequired();

        builder.Property(m => m.MatchedAt)
            .IsRequired();

        builder.Property(m => m.MatchConfidence)
            .HasConversion<string>()
            .HasMaxLength(MatchConfidenceMaxLength)
            .IsRequired();

        builder.HasOne(m => m.Receipt)
            .WithOne()
            .HasForeignKey<MatchResult>(m => m.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.BankTransaction)
            .WithOne()
            .HasForeignKey<MatchResult>(m => m.BankTransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.ReceiptId).IsUnique();
        builder.HasIndex(m => m.BankTransactionId).IsUnique();
    }
}
