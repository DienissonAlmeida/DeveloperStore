using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CreateSale;

public sealed record CreateSaleCommand(
    string SaleNumber,
    DateTime SaleDate,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    IReadOnlyList<CreateSaleItemDto> Items) : IRequest<Guid>;

public sealed record CreateSaleItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Discount);
