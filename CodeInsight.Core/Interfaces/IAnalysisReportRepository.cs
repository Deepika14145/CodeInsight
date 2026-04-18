using CodeInsight.Core.Entities;

namespace CodeInsight.Core.Interfaces;

public interface IAnalysisReportRepository
{
    Task<AnalysisReport?> GetByIdAsync(int id);
    Task<AnalysisReport?> GetByIdWithIssuesAsync(int id);
    Task<List<AnalysisReport>> GetAllAsync(int page, int pageSize);
    Task<int> GetTotalCountAsync();
    Task<AnalysisReport> AddAsync(AnalysisReport report);
    Task<AnalysisReport> UpdateAsync(AnalysisReport report);
    Task<List<AnalysisReport>> GetByRepositoryIdAsync(int repositoryId);
}
