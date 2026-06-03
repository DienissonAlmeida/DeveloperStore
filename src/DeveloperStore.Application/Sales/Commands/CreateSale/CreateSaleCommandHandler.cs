using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.Domain.ValueObjects;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CreateSale;

public sealed class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Guid>
{
    private readonly ISaleRepository _repository;

    public CreateSaleCommandHandler(ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = Sale.Create(
            request.SaleNumber,
            request.SaleDate,
            new ExternalIdentity(request.CustomerId, request.CustomerName),
            new ExternalIdentity(request.BranchId, request.BranchName));

        foreach (var item in request.Items)
            sale.AddItem(
                new ExternalIdentity(item.ProductId, item.ProductName),
                item.Quantity,
                item.UnitPrice,
                CalculateDiscount(item.Quantity));

        await _repository.AddAsync(sale, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return sale.Id;
    }

    private static decimal CalculateDiscount(int quantity) =>
        quantity >= 4 ? 0.10m : 0m;
}
