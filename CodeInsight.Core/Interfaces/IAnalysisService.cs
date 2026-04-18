using CodeInsight.Core.DTOs;

namespace CodeInsight.Core.Interfaces;

public interface IAnalysisService
{
    Task<AnalysisReportSummaryDto> StartAnalysisAsync(AnalyzeRequestDto request);
    Task<AnalysisReportDto?> GetReportAsync(int reportId);
    Task<PagedResult<AnalysisReportSummaryDto>> GetAllReportsAsync(int page, int pageSize);
    Task<PagedResult<CodeIssueDto>> GetIssuesAsync(int reportId, int page, int pageSize);
}
