namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IReceiptStorage
{
    Task SaveReceiptAsync(Receipt receipt);
    Task<IEnumerable<Receipt>> GetAllReceiptsAsync();
}
