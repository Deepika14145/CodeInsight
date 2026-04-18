using CodeInsight.Core.Entities;

namespace CodeInsight.Core.Interfaces;

public interface ICodeIssueRepository
{
    Task<List<CodeIssue>> GetByReportIdAsync(int reportId, int page, int pageSize);
    Task<int> GetCountByReportIdAsync(int reportId);
    Task AddRangeAsync(IEnumerable<CodeIssue> issues);
}
