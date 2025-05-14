using TaxReturnAutomation.Application.Common.DTOs;

namespace TaxReturnAutomation.Infrastructure.Parsing;
public class AzureFormRecongnizerInvoiceParser : IInvoiceParser
{
    public Task<InvoiceDto> ParseAsync(Uri fileUri, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
