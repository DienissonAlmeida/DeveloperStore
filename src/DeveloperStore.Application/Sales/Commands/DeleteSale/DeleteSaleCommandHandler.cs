using DeveloperStore.Domain.Interfaces;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.DeleteSale;

public sealed class DeleteSaleCommandHandler : IRequestHandler<DeleteSaleCommand, bool>
{
    private readonly ISaleRepository _repository;

    public DeleteSaleCommandHandler(ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
            return false;

        _repository.Remove(sale);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
