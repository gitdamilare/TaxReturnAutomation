namespace TaxReturnAutomation.Infrastructure.Data.Configurations;
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        const string TableName = "Invoices";
        const int FileNameMaxLength = 255;
        const int DescriptionMaxLength = 2000;
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

        builder.Property(r => r.InvoiceNumber)
            .HasMaxLength(CommonMaxLength);

        builder.Property(r => r.CustomerName)
            .HasMaxLength(CommonMaxLength);
    }
}
