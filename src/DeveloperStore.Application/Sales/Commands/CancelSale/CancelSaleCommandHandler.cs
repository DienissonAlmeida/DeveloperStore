using DeveloperStore.Application.Common.Interfaces;
using DeveloperStore.Application.Events;
using DeveloperStore.Domain.Interfaces;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.CancelSale;

public sealed class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, bool>
{
    private readonly ISaleRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public CancelSaleCommandHandler(ISaleRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
            return false;

        sale.Cancel();
        _repository.Update(sale);
        await _repository.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(new SaleCancelled(
            sale.Id,
            sale.SaleNumber,
            sale.UpdatedAt ?? DateTime.UtcNow), cancellationToken);

        return true;
    }
}
