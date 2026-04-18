namespace CodeInsight.Core.DTOs;

public class AnalysisReportDto
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalFilesAnalyzed { get; set; }
    public int TotalIssuesFound { get; set; }
    public int HighComplexityCount { get; set; }
    public int LongMethodCount { get; set; }
    public int DuplicateBlockCount { get; set; }
    public int DeepNestingCount { get; set; }
    public double MaintainabilityScore { get; set; }
    public double ComplexityScore { get; set; }
    public double CodeQualityScore { get; set; }
    public int ProgressPercent { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<CodeIssueDto> Issues { get; set; } = new();
}

public class AnalysisReportSummaryDto
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalFilesAnalyzed { get; set; }
    public int TotalIssuesFound { get; set; }
    public double MaintainabilityScore { get; set; }
    public double CodeQualityScore { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
