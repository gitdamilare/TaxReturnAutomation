namespace TaxReturnAutomation.Domain.Entities;
public class ProcessedFile
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FileName { get; private set; } = string.Empty;
    public FileType FileType { get; private set; }
    public ProcessStatus Status { get; private set; }
    public DateTime ProcessedAtUtc { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    public static ProcessedFile Create(
        string fileName,
        FileType fileType,
        ProcessStatus status,
        string errorMessage = "")
    {
        ArgumentNullException.ThrowIfNull(fileName);
        return new ProcessedFile
        {
            FileName = fileName,
            FileType = fileType,
            Status = status,
            ProcessedAtUtc = DateTime.UtcNow,
            ErrorMessage = errorMessage
        };
    }
}
