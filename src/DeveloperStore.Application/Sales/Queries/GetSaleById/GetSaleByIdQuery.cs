using DeveloperStore.Application.Sales.DTOs;
using MediatR;

namespace DeveloperStore.Application.Sales.Queries.GetSaleById;

public sealed record GetSaleByIdQuery(Guid Id) : IRequest<SaleDto?>;
