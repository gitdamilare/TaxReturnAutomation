namespace TaxReturnAutomation.Infrastructure.Data.Configurations;
public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> builder)
    {
        const string TableName = "BankTransactions";
        const int DescriptionMaxLength = 500;
        const int TransactionTypeMaxLength = 20;


        builder.ToTable(TableName);

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount).IsRequired();

        builder.Property(b => b.TransactionDate).IsRequired();

        builder.Property(b => b.Description)
            .IsRequired()
            .HasMaxLength(DescriptionMaxLength);

        builder.Property(b => b.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(TransactionTypeMaxLength)
            .IsRequired();
    }
}
