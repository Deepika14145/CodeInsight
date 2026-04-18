using Microsoft.EntityFrameworkCore;
using CodeInsight.Core.Entities;
using CodeInsight.Core.Interfaces;
using CodeInsight.Infrastructure.Data;

namespace CodeInsight.Infrastructure.Repositories;

public class CodeIssueRepository : ICodeIssueRepository
{
    private readonly AppDbContext _db;

    public CodeIssueRepository(AppDbContext db) => _db = db;

    public async Task<List<CodeIssue>> GetByReportIdAsync(int reportId, int page, int pageSize) =>
        await _db.CodeIssues
            .Where(i => i.ReportId == reportId)
            .OrderBy(i => i.Severity == "Critical" ? 0 : i.Severity == "High" ? 1 : i.Severity == "Medium" ? 2 : 3)
            .ThenBy(i => i.FileName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetCountByReportIdAsync(int reportId) =>
        await _db.CodeIssues.CountAsync(i => i.ReportId == reportId);

    public async Task AddRangeAsync(IEnumerable<CodeIssue> issues)
    {
        await _db.CodeIssues.AddRangeAsync(issues);
        await _db.SaveChangesAsync();
    }
}
