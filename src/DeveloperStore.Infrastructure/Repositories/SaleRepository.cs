using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;

namespace DeveloperStore.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private static readonly Dictionary<Guid, Sale> _store = [];

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.GetValueOrDefault(id));

    public Task<IEnumerable<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_store.Values.AsEnumerable());

    public Task AddAsync(Sale entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public void Update(Sale entity)
    {
        _store[entity.Id] = entity;
    }

    public void Remove(Sale entity)
    {
        _store.Remove(entity.Id);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(1);

    public Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.Values.FirstOrDefault(s => s.SaleNumber == saleNumber));
}
