namespace TaxReturnAutomation.Infrastructure.Data.Configurations;
public class ProcessedFileConfiguration : IEntityTypeConfiguration<ProcessedFile>
{
    public void Configure(EntityTypeBuilder<ProcessedFile> builder)
    {
        const string TableName = "ProcessedFiles";
        const int FileNameMaxLength = 255;

        builder.ToTable(TableName);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(FileNameMaxLength);

        builder.Property(p => p.ProcessedAtUtc)
            .IsRequired();

        builder.Property(p => p.FileType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
    }
}
