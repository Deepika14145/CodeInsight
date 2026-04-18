using Microsoft.EntityFrameworkCore;
using CodeInsight.Core.Entities;
using CodeInsight.Core.Interfaces;
using CodeInsight.Infrastructure.Data;

namespace CodeInsight.Infrastructure.Repositories;

public class AnalysisReportRepository : IAnalysisReportRepository
{
    private readonly AppDbContext _db;

    public AnalysisReportRepository(AppDbContext db) => _db = db;

    public async Task<AnalysisReport?> GetByIdAsync(int id) =>
        await _db.AnalysisReports.Include(r => r.Repository).FirstOrDefaultAsync(r => r.Id == id);

    public async Task<AnalysisReport?> GetByIdWithIssuesAsync(int id) =>
        await _db.AnalysisReports
            .Include(r => r.Repository)
            .Include(r => r.Issues)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<AnalysisReport>> GetAllAsync(int page, int pageSize) =>
        await _db.AnalysisReports
            .Include(r => r.Repository)
            .OrderByDescending(r => r.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetTotalCountAsync() =>
        await _db.AnalysisReports.CountAsync();

    public async Task<AnalysisReport> AddAsync(AnalysisReport report)
    {
        _db.AnalysisReports.Add(report);
        await _db.SaveChangesAsync();
        return report;
    }

    public async Task<AnalysisReport> UpdateAsync(AnalysisReport report)
    {
        _db.AnalysisReports.Update(report);
        await _db.SaveChangesAsync();
        return report;
    }

    public async Task<List<AnalysisReport>> GetByRepositoryIdAsync(int repositoryId) =>
        await _db.AnalysisReports
            .Where(r => r.RepositoryId == repositoryId)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync();
}
