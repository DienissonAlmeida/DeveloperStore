using DeveloperStore.Application.Common.Interfaces;
using DeveloperStore.Application.Events;
using DeveloperStore.Domain.Interfaces;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.DeleteSale;

public sealed class DeleteSaleCommandHandler : IRequestHandler<DeleteSaleCommand, bool>
{
    private readonly ISaleRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public DeleteSaleCommandHandler(ISaleRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
            return false;

        _repository.Remove(sale);
        await _repository.SaveChangesAsync(cancellationToken);

        foreach (var item in sale.Items)
            await _eventPublisher.PublishAsync(new ItemCancelled(
                sale.Id,
                item.Id,
                item.Product.Id,
                item.Product.Name,
                DateTime.UtcNow), cancellationToken);

        await _eventPublisher.PublishAsync(new SaleCancelled(
            sale.Id,
            sale.SaleNumber,
            DateTime.UtcNow), cancellationToken);

        return true;
    }
}
