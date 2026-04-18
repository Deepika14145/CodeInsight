using CodeInsight.Core.Entities;
using CodeInsight.Core.Interfaces;
using CodeInsight.Analysis.Analyzers;
using Microsoft.Extensions.Logging;

namespace CodeInsight.Analysis.Engine;

/// <summary>
/// Orchestrates full repository analysis: discovers .cs files, runs all analyzers,
/// computes scores, and updates the report with progress.
/// </summary>
public class RepositoryAnalysisOrchestrator
{
    private readonly ICodeAnalyzer _fileAnalyzer;
    private readonly IAnalysisReportRepository _reportRepository;
    private readonly ICodeIssueRepository _issueRepository;
    private readonly ILogger<RepositoryAnalysisOrchestrator> _logger;

    public RepositoryAnalysisOrchestrator(
        ICodeAnalyzer fileAnalyzer,
        IAnalysisReportRepository reportRepository,
        ICodeIssueRepository issueRepository,
        ILogger<RepositoryAnalysisOrchestrator> logger)
    {
        _fileAnalyzer = fileAnalyzer;
        _reportRepository = reportRepository;
        _issueRepository = issueRepository;
        _logger = logger;
    }

    public async Task RunAsync(int reportId, string repoPath, AnalysisOptions options)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null) return;

        try
        {
            report.Status = "Running";
            report.ProgressPercent = 5;
            await _reportRepository.UpdateAsync(report);

            // List all files for debugging
            var allFiles = Directory.GetFiles(repoPath, "*.*", SearchOption.AllDirectories).Length;
            _logger.LogInformation("Total files in repo: {Total}", allFiles);

            var csFiles = Directory.GetFiles(repoPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("obj" + Path.DirectorySeparatorChar)
                         && !f.Contains("bin" + Path.DirectorySeparatorChar)
                         && !f.Contains(".git" + Path.DirectorySeparatorChar))
                .Take(200)  // Cap at 200 files to keep analysis fast
                .ToList();

            _logger.LogInformation("Found {Count} C# files in {Path}", csFiles.Count, repoPath);

            if (csFiles.Count == 0)
            {
                report.Status = "Completed";
                report.ProgressPercent = 100;
                report.TotalFilesAnalyzed = 0;
                report.TotalIssuesFound = 0;
                report.MaintainabilityScore = 100;
                report.ComplexityScore = 100;
                report.CodeQualityScore = 100;
                report.ErrorMessage = "No C# (.cs) files found in this repository.";
                report.CompletedAt = DateTime.UtcNow;
                await _reportRepository.UpdateAsync(report);
                return;
            }

            var allIssues = new List<CodeIssue>();
            var duplicateState = new Dictionary<string, (string, string, string, int)>();
            var duplicateAnalyzer = new DuplicateCodeAnalyzer(duplicateState);

            for (int i = 0; i < csFiles.Count; i++)
            {
                var file = csFiles[i];
                var fileIssues = await _fileAnalyzer.AnalyzeFileAsync(file, reportId, options);

                // Run duplicate detection separately (needs shared state)
                var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
                    await File.ReadAllTextAsync(file));
                var dupIssues = duplicateAnalyzer.Analyze(tree, file, Path.GetFileName(file), reportId);
                fileIssues.AddRange(dupIssues);

                allIssues.AddRange(fileIssues);

                // Update progress
                report.ProgressPercent = 5 + (int)((double)(i + 1) / csFiles.Count * 85);
                await _reportRepository.UpdateAsync(report);
            }

            // Persist issues in batches
            if (allIssues.Count > 0)
                await _issueRepository.AddRangeAsync(allIssues);

            // Compute scores
            report.TotalFilesAnalyzed = csFiles.Count;
            report.TotalIssuesFound = allIssues.Count;
            report.HighComplexityCount = allIssues.Count(i => i.IssueType == "CyclomaticComplexity");
            report.LongMethodCount = allIssues.Count(i => i.IssueType == "LongMethod");
            report.DuplicateBlockCount = allIssues.Count(i => i.IssueType == "DuplicateCode");
            report.DeepNestingCount = allIssues.Count(i => i.IssueType == "DeepNesting");
            report.MaintainabilityScore = ComputeMaintainabilityScore(allIssues, csFiles.Count);
            report.ComplexityScore = ComputeComplexityScore(allIssues, csFiles.Count);
            report.CodeQualityScore = (report.MaintainabilityScore + report.ComplexityScore) / 2.0;
            report.Status = "Completed";
            report.ProgressPercent = 100;
            report.CompletedAt = DateTime.UtcNow;

            await _reportRepository.UpdateAsync(report);
            _logger.LogInformation("Analysis completed for report {ReportId}. Issues: {Count}", reportId, allIssues.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Analysis failed for report {ReportId}", reportId);
            report.Status = "Failed";
            report.ErrorMessage = ex.Message;
            report.CompletedAt = DateTime.UtcNow;
            await _reportRepository.UpdateAsync(report);
        }
        finally
        {
            // Clean up cloned repo
            try
            {
                if (Directory.Exists(repoPath))
                    Directory.Delete(repoPath, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not clean up temp directory: {Path}", repoPath);
            }
        }
    }

    private static double ComputeMaintainabilityScore(List<CodeIssue> issues, int fileCount)
    {
        if (fileCount == 0) return 100;
        double criticalWeight = issues.Count(i => i.Severity == "Critical") * 5.0;
        double highWeight = issues.Count(i => i.Severity == "High") * 3.0;
        double mediumWeight = issues.Count(i => i.Severity == "Medium") * 1.5;
        double lowWeight = issues.Count(i => i.Severity == "Low") * 0.5;
        double penalty = (criticalWeight + highWeight + mediumWeight + lowWeight) / fileCount;
        return Math.Max(0, Math.Round(100 - penalty, 1));
    }

    private static double ComputeComplexityScore(List<CodeIssue> issues, int fileCount)
    {
        if (fileCount == 0) return 100;
        var complexityIssues = issues.Where(i => i.IssueType == "CyclomaticComplexity").ToList();
        double penalty = complexityIssues.Sum(i => (i.MetricValue ?? 10) - 10) / (double)fileCount;
        return Math.Max(0, Math.Round(100 - penalty * 2, 1));
    }
}
