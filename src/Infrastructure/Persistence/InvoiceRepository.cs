namespace TaxReturnAutomation.Infrastructure.Persistence;
internal class InvoiceRepository : IInvoiceStorage
{
    public Task SaveInvoiceAsync(Invoice invoice)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
    {
        throw new NotImplementedException();
    }
}
