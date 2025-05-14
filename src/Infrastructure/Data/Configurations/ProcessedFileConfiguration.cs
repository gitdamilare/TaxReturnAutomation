namespace TaxReturnAutomation.Infrastructure.Data.Configurations;
public class ProcessedFileConfiguration : IEntityTypeConfiguration<ProcessedFile>
{
    public void Configure(EntityTypeBuilder<ProcessedFile> builder)
    {
        const string TableName = "ProcessedFiles";
        const int FileNameMaxLength = 255;
        const int EnumMaxLength = 20;

        builder.ToTable(TableName);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FileName)
            .IsRequired()
            .HasMaxLength(FileNameMaxLength);

        builder.Property(p => p.ProcessedAtUtc)
            .IsRequired();

        builder.Property(p => p.FileType)
            .HasConversion<string>()
            .HasMaxLength(EnumMaxLength)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(EnumMaxLength)
            .IsRequired();
    }
}
