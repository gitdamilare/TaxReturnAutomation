namespace TaxReturnAutomation.Infrastructure.Data.Configurations;
public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        const string TableName = "Receipts";
        const int FileNameMaxLength = 255;
        const int DescriptionMaxLength = 500;
        const int CommonMaxLength = 100;

        builder.ToTable(TableName);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.FileName)
            .IsRequired()
            .HasMaxLength(FileNameMaxLength);

        builder.Property(r => r.UploadedAt)
            .IsRequired();

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(DescriptionMaxLength);

        builder.Property(r => r.ReceiptNumber)
            .HasMaxLength(CommonMaxLength);

        builder.Property(r => r.CustomerName)
            .HasMaxLength(CommonMaxLength);
    }
}
