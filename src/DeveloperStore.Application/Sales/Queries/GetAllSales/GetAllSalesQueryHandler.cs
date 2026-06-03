using DeveloperStore.Application.Sales.DTOs;
using DeveloperStore.Domain.Interfaces;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetAllSales;

public sealed class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, IEnumerable<SaleDto>>
{
    private readonly ISaleRepository _repository;

    public GetAllSalesQueryHandler(ISaleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SaleDto>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {
        var sales = await _repository.GetAllAsync(cancellationToken);
        return sales.Select(SaleDto.FromEntity);
    }
}
