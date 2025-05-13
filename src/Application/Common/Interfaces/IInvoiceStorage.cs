namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IInvoiceStorage
{
    Task SaveInvoiceAsync(Invoice invoice);
    Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
}
