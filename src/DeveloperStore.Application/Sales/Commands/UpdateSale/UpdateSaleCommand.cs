using DeveloperStore.Application.Sales.Commands.CreateSale;
using DeveloperStore.Application.Sales.DTOs;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.UpdateSale;

public sealed record UpdateSaleCommand(
    Guid Id,
    string SaleNumber,
    DateTime SaleDate,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    IReadOnlyList<CreateSaleItemDto> Items) : IRequest<SaleDto?>;
