using CodeInsight.Core.DTOs;
using CodeInsight.Core.Entities;
using CodeInsight.Core.Interfaces;
using CodeInsight.Analysis.Engine;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CodeInsight.API.Services;

public class AnalysisService : IAnalysisService
{
    private readonly IRepositoryRepository _repoRepo;
    private readonly IAnalysisReportRepository _reportRepo;
    private readonly ICodeIssueRepository _issueRepo;
    private readonly IGitHubService _githubService;
    private readonly RepositoryAnalysisOrchestrator _orchestrator;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AnalysisService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AnalysisService(
        IRepositoryRepository repoRepo,
        IAnalysisReportRepository reportRepo,
        ICodeIssueRepository issueRepo,
        IGitHubService githubService,
        RepositoryAnalysisOrchestrator orchestrator,
        IMemoryCache cache,
        ILogger<AnalysisService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _repoRepo = repoRepo;
        _reportRepo = reportRepo;
        _issueRepo = issueRepo;
        _githubService = githubService;
        _orchestrator = orchestrator;
        _cache = cache;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task<AnalysisReportSummaryDto> StartAnalysisAsync(AnalyzeRequestDto request)
    {
        // Fetch or create repository record
        var repo = await _repoRepo.GetByUrlAsync(request.RepositoryUrl);
        if (repo == null)
        {
            var metadata = await _githubService.GetRepositoryMetadataAsync(request.RepositoryUrl);
            repo = new Core.Entities.Repository
            {
                Url = request.RepositoryUrl,
                Name = metadata?.Name ?? ExtractRepoName(request.RepositoryUrl),
                Owner = metadata?.Owner,
                Description = metadata?.Description,
                Language = metadata?.Language,
                Stars = metadata?.Stars
            };
            repo = await _repoRepo.AddAsync(repo);
        }

        // Create a pending report
        var report = new AnalysisReport
        {
            RepositoryId = repo.Id,
            Status = "Pending",
            ProgressPercent = 0
        };
        report = await _reportRepo.AddAsync(report);

        var tempPath = Path.Combine(Path.GetTempPath(), "codeinsight", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);

        var options = new AnalysisOptions
        {
            LongMethodThreshold = request.LongMethodThreshold,
            ComplexityThreshold = request.ComplexityThreshold,
            NestingThreshold = request.NestingThreshold
        };

        int reportId = report.Id;
        string repoUrl = request.RepositoryUrl;

        // Background task with its own DI scope to avoid disposed DbContext
        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var githubSvc = scope.ServiceProvider.GetRequiredService<IGitHubService>();
            var orchestrator = scope.ServiceProvider.GetRequiredService<RepositoryAnalysisOrchestrator>();
            try
            {
                await githubSvc.CloneRepositoryAsync(repoUrl, tempPath);
                await orchestrator.RunAsync(reportId, tempPath, options);
                _cache.Remove($"report_{reportId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background analysis failed for report {Id}", reportId);
            }
        });

        return MapToSummary(report, repo);
    }

    public async Task<AnalysisReportDto?> GetReportAsync(int reportId)
    {
        // Always fetch fresh data — don't serve stale cached pending/running reports
        var report = await _reportRepo.GetByIdWithIssuesAsync(reportId);
        if (report == null) return null;

        var dto = MapToDto(report);

        // Only cache completed reports for 5 minutes
        if (report.Status == "Completed")
        {
            var cacheKey = $"report_{reportId}";
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));
        }

        return dto;
    }

    public async Task<PagedResult<AnalysisReportSummaryDto>> GetAllReportsAsync(int page, int pageSize)
    {
        var reports = await _reportRepo.GetAllAsync(page, pageSize);
        var total = await _reportRepo.GetTotalCountAsync();

        return new PagedResult<AnalysisReportSummaryDto>
        {
            Items = reports.Select(r => MapToSummary(r, r.Repository)).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<CodeIssueDto>> GetIssuesAsync(int reportId, int page, int pageSize)
    {
        var issues = await _issueRepo.GetByReportIdAsync(reportId, page, pageSize);
        var total = await _issueRepo.GetCountByReportIdAsync(reportId);

        return new PagedResult<CodeIssueDto>
        {
            Items = issues.Select(MapIssueToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private static AnalysisReportSummaryDto MapToSummary(AnalysisReport r, Core.Entities.Repository repo) => new()
    {
        Id = r.Id,
        RepositoryId = r.RepositoryId,
        RepositoryName = repo.Name,
        RepositoryUrl = repo.Url,
        Status = r.Status,
        TotalFilesAnalyzed = r.TotalFilesAnalyzed,
        TotalIssuesFound = r.TotalIssuesFound,
        MaintainabilityScore = r.MaintainabilityScore,
        CodeQualityScore = r.CodeQualityScore,
        ProgressPercent = r.ProgressPercent,
        StartedAt = r.StartedAt,
        CompletedAt = r.CompletedAt
    };

    private static AnalysisReportDto MapToDto(AnalysisReport r) => new()
    {
        Id = r.Id,
        RepositoryId = r.RepositoryId,
        RepositoryName = r.Repository.Name,
        RepositoryUrl = r.Repository.Url,
        Status = r.Status,
        TotalFilesAnalyzed = r.TotalFilesAnalyzed,
        TotalIssuesFound = r.TotalIssuesFound,
        HighComplexityCount = r.HighComplexityCount,
        LongMethodCount = r.LongMethodCount,
        DuplicateBlockCount = r.DuplicateBlockCount,
        DeepNestingCount = r.DeepNestingCount,
        MaintainabilityScore = r.MaintainabilityScore,
        ComplexityScore = r.ComplexityScore,
        CodeQualityScore = r.CodeQualityScore,
        ProgressPercent = r.ProgressPercent,
        ErrorMessage = r.ErrorMessage,
        StartedAt = r.StartedAt,
        CompletedAt = r.CompletedAt,
        Issues = r.Issues.Select(MapIssueToDto).ToList()
    };

    private static CodeIssueDto MapIssueToDto(CodeIssue i) => new()
    {
        Id = i.Id,
        ReportId = i.ReportId,
        FilePath = i.FilePath,
        FileName = i.FileName,
        IssueType = i.IssueType,
        Severity = i.Severity,
        Description = i.Description,
        Suggestion = i.Suggestion,
        MethodName = i.MethodName,
        LineStart = i.LineStart,
        LineEnd = i.LineEnd,
        MetricValue = i.MetricValue,
        CodeSnippet = i.CodeSnippet
    };

    private static string ExtractRepoName(string url)
    {
        var parts = url.TrimEnd('/').Split('/');
        return parts.LastOrDefault()?.Replace(".git", "") ?? "Unknown";
    }
}
