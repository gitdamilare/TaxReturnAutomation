namespace TaxReturnAutomation.Infrastructure.Data.Configurations;
public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        const string tableName = "Invoices";
        const int fileNameMaxLength = 255;
        const int descriptionMaxLength = 2000;
        const int commonMaxLength = 100;

        builder.ToTable(tableName);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.FileName)
            .IsRequired()
            .HasMaxLength(fileNameMaxLength);

        builder.Property(r => r.UploadedAt)
            .IsRequired();

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(descriptionMaxLength);

        builder.Property(r => r.InvoiceNumber)
            .HasMaxLength(commonMaxLength);

        builder.Property(r => r.CustomerName)
            .HasMaxLength(commonMaxLength);

        builder.Property(r => r.CustomerId)
            .HasMaxLength(commonMaxLength);
    }
}
