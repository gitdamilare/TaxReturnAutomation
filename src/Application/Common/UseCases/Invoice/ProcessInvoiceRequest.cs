using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxReturnAutomation.Application.Common.UseCases.Invoice;
public record ProcessInvoiceRequest(
    string FileName,
    Uri FileUri);
