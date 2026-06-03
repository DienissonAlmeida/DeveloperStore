using DeveloperStore.Application.Common.Interfaces;
using DeveloperStore.Application.Events;
using DeveloperStore.Domain.Interfaces;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CancelSaleItem;

public sealed class CancelSaleItemCommandHandler : IRequestHandler<CancelSaleItemCommand, bool>
{
    private readonly ISaleRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public CancelSaleItemCommandHandler(ISaleRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
            return false;

        sale.CancelItem(request.ItemId);
        _repository.Update(sale);
        await _repository.SaveChangesAsync(cancellationToken);

        var item = sale.Items.First(i => i.Id == request.ItemId);

        await _eventPublisher.PublishAsync(new ItemCancelled(
            sale.Id,
            item.Id,
            item.Product.Id,
            item.Product.Name,
            item.UpdatedAt ?? DateTime.UtcNow), cancellationToken);

        return true;
    }
}
