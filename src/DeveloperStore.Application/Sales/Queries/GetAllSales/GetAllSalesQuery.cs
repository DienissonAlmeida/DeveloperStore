using DeveloperStore.Application.Sales.DTOs;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetAllSales;

public sealed record GetAllSalesQuery : IRequest<IEnumerable<SaleDto>>;
