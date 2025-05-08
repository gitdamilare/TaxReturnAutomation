namespace TaxReturnAutomation.Domain.Entities;
public class ProcessedFile
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FileName { get; private set; } = string.Empty;
    public FileType FileType { get; private set; }
    public DateTime ProcessedAtUtc { get; private set; }

    private ProcessedFile() { }

    public static ProcessedFile Create(string fileName, FileType fileType)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        return new ProcessedFile
        {
            FileName = fileName,
            FileType = fileType,
            ProcessedAtUtc = DateTime.UtcNow
        };
    }
}
