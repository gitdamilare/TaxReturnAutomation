namespace TaxReturnAutomation.Infrastructure.Data.Configurations;
public class BankStatmentConfiguration : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        const string TableName = "BankStatements";
        const int FileNameMaxLength = 255;

        builder.ToTable(TableName);

        builder.HasKey(b => b.Id);

        builder.Property(b => b.FileName)
            .IsRequired()
            .HasMaxLength(FileNameMaxLength);

        builder.Property(b => b.UploadedAt)
            .IsRequired();

        builder.HasMany(b => b.Transactions)
            .WithOne()
            .HasForeignKey(p => p.BankStatementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
