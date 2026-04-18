using Microsoft.EntityFrameworkCore;
using CodeInsight.Core.Interfaces;
using CodeInsight.Infrastructure.Data;

namespace CodeInsight.Infrastructure.Repositories;

public class RepositoryRepository : IRepositoryRepository
{
    private readonly AppDbContext _db;

    public RepositoryRepository(AppDbContext db) => _db = db;

    public async Task<Core.Entities.Repository?> GetByUrlAsync(string url) =>
        await _db.Repositories.FirstOrDefaultAsync(r => r.Url == url);

    public async Task<Core.Entities.Repository?> GetByIdAsync(int id) =>
        await _db.Repositories.FindAsync(id);

    public async Task<List<Core.Entities.Repository>> GetAllAsync() =>
        await _db.Repositories.OrderByDescending(r => r.CreatedAt).ToListAsync();

    public async Task<Core.Entities.Repository> AddAsync(Core.Entities.Repository repository)
    {
        _db.Repositories.Add(repository);
        await _db.SaveChangesAsync();
        return repository;
    }

    public async Task<Core.Entities.Repository> UpdateAsync(Core.Entities.Repository repository)
    {
        _db.Repositories.Update(repository);
        await _db.SaveChangesAsync();
        return repository;
    }
}
