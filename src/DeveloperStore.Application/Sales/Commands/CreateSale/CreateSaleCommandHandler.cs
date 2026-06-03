using DeveloperStore.Application.Common.Interfaces;
using DeveloperStore.Application.Events;
using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.Domain.ValueObjects;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CreateSale;

public sealed class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Guid>
{
    private readonly ISaleRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public CreateSaleCommandHandler(ISaleRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
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

        await _eventPublisher.PublishAsync(new SaleCreated(
            sale.Id,
            sale.SaleNumber,
            sale.Customer.Id,
            sale.Customer.Name,
            sale.Branch.Id,
            sale.Branch.Name,
            sale.TotalAmount,
            sale.CreatedAt), cancellationToken);

        return sale.Id;
    }

    private static decimal CalculateDiscount(int quantity)
    {
        if (quantity > 20)
            throw new InvalidOperationException("Cannot sell more than 20 identical items.");

        if (quantity >= 10)
            return 0.20m;

        if (quantity >= 4)
            return 0.10m;

        return 0m;
    }
}
