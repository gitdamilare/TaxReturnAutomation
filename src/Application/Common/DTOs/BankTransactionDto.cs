using TaxReturnAutomation.Domain.Enums;

namespace TaxReturnAutomation.Application.Common.DTOs;
public record BankTransactionDto
(
    DateTime BookingDate,
    DateTime ValueDate,
    string Description,
    decimal Amount,
    TransactionType type
);
