namespace CodeInsight.Core.Entities;

public class AnalysisReport
{
    public int Id { get; set; }
    public int RepositoryId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Running, Completed, Failed
    public int TotalFilesAnalyzed { get; set; }
    public int TotalIssuesFound { get; set; }
    public int HighComplexityCount { get; set; }
    public int LongMethodCount { get; set; }
    public int DuplicateBlockCount { get; set; }
    public int DeepNestingCount { get; set; }
    public double MaintainabilityScore { get; set; } // 0-100
    public double ComplexityScore { get; set; }
    public double CodeQualityScore { get; set; }
    public string? ErrorMessage { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public Repository Repository { get; set; } = null!;
    public ICollection<CodeIssue> Issues { get; set; } = new List<CodeIssue>();
}
