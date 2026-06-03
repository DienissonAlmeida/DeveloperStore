using DeveloperStore.Application.Common.Interfaces;
using DeveloperStore.Application.Events;
using DeveloperStore.Application.Sales.DTOs;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.Domain.ValueObjects;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.UpdateSale;

public sealed class UpdateSaleCommandHandler : IRequestHandler<UpdateSaleCommand, SaleDto?>
{
    private readonly ISaleRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public UpdateSaleCommandHandler(ISaleRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<SaleDto?> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
            return null;

        sale.Update(
            request.SaleNumber,
            request.SaleDate,
            new ExternalIdentity(request.CustomerId, request.CustomerName),
            new ExternalIdentity(request.BranchId, request.BranchName));

        sale.ReplaceItems(request.Items.Select(i =>
            (new ExternalIdentity(i.ProductId, i.ProductName),
             i.Quantity,
             i.UnitPrice,
             i.Discount)));

        _repository.Update(sale);
        await _repository.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new SaleModified(
            sale.Id,
            sale.SaleNumber,
            sale.Customer.Id,
            sale.Customer.Name,
            sale.Branch.Id,
            sale.Branch.Name,
            sale.TotalAmount,
            sale.UpdatedAt ?? DateTime.UtcNow), cancellationToken);

        return SaleDto.FromEntity(sale);
    }
}
