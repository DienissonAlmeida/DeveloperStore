using DeveloperStore.Application.Sales.DTOs;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.Domain.ValueObjects;
using MediatR;

namespace DeveloperStore.Application.Sales.Commands.UpdateSale;

public sealed class UpdateSaleCommandHandler : IRequestHandler<UpdateSaleCommand, SaleDto?>
{
    private readonly ISaleRepository _repository;

    public UpdateSaleCommandHandler(ISaleRepository repository)
    {
        _repository = repository;
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

        return SaleDto.FromEntity(sale);
    }
}
