namespace DeveloperStore.Application.Events;

public record SaleCreated(
    Guid SaleId,
    string SaleNumber,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    decimal TotalAmount,
    DateTime CreatedAt);
