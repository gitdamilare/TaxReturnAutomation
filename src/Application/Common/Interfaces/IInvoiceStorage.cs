namespace TaxReturnAutomation.Application.Common.Interfaces;
public interface IInvoiceStorage
{
    Task SaveInvoiceAsync(Invoice invoice, CancellationToken cancellationToken);
    Task<List<Invoice>> GetAllInvoicesAsync();
}
