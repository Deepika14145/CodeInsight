using CodeInsight.Core.Entities;

namespace CodeInsight.Core.Interfaces;

public interface IRepositoryRepository
{
    Task<Entities.Repository?> GetByUrlAsync(string url);
    Task<Entities.Repository?> GetByIdAsync(int id);
    Task<List<Entities.Repository>> GetAllAsync();
    Task<Entities.Repository> AddAsync(Entities.Repository repository);
    Task<Entities.Repository> UpdateAsync(Entities.Repository repository);
}
