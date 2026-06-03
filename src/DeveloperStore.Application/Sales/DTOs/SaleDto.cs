using DeveloperStore.Domain.Entities;

namespace DeveloperStore.Application.Sales.DTOs;

public sealed record SaleDto(
    Guid Id,
    string SaleNumber,
    DateTime SaleDate,
    Guid CustomerId,
    string CustomerName,
    Guid BranchId,
    string BranchName,
    decimal TotalAmount,
    bool IsCancelled,
    IReadOnlyList<SaleItemDto> Items,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static SaleDto FromEntity(Sale sale) => new(
        sale.Id,
        sale.SaleNumber,
        sale.SaleDate,
        sale.Customer.Id,
        sale.Customer.Name,
        sale.Branch.Id,
        sale.Branch.Name,
        sale.TotalAmount,
        sale.IsCancelled,
        sale.Items.Select(SaleItemDto.FromEntity).ToList(),
        sale.CreatedAt,
        sale.UpdatedAt);
}

public sealed record SaleItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal TotalAmount)
{
    public static SaleItemDto FromEntity(SaleItem item) => new(
        item.Id,
        item.Product.Id,
        item.Product.Name,
        item.Quantity,
        item.UnitPrice,
        item.Discount,
        item.TotalAmount);
}
