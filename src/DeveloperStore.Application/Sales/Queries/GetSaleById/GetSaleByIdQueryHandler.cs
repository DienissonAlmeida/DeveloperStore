using DeveloperStore.Application.Sales.DTOs;
using DeveloperStore.Domain.Interfaces;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetSaleById;

public sealed class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, SaleDto?>
{
    private readonly ISaleRepository _repository;

    public GetSaleByIdQueryHandler(ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<SaleDto?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return sale is null ? null : SaleDto.FromEntity(sale);
    }
}
