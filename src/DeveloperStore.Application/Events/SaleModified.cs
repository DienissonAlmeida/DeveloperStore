namespace DeveloperStore.Application.Events;

public record SaleModified(
    Guid SaleId,
    string SaleNumber,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    decimal TotalAmount,
    DateTime ModifiedAt);
