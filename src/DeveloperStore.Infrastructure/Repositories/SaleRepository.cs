using DeveloperStore.Domain.Entities;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Infrastructure.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly AppDbContext _context;

    public SaleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IEnumerable<Sale>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Sales
            .Include(s => s.Items)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Sale entity, CancellationToken cancellationToken = default)
        => await _context.Sales.AddAsync(entity, cancellationToken);

    public void Update(Sale entity)
    {
        var existingItems = _context.SaleItems
            .Where(i => i.SaleId == entity.Id)
            .ToList();

        _context.SaleItems.RemoveRange(existingItems);
        _context.SaleItems.AddRange(entity.Items);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Remove(Sale entity)
        => _context.Sales.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
        => await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
}
