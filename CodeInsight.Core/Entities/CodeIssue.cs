namespace CodeInsight.Core.Entities;

public class CodeIssue
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;   // CyclomaticComplexity, LongMethod, DeepNesting, DuplicateCode, NamingConvention
    public string Severity { get; set; } = string.Empty;    // Critical, High, Medium, Low
    public string Description { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string? MethodName { get; set; }
    public int? LineStart { get; set; }
    public int? LineEnd { get; set; }
    public int? MetricValue { get; set; }   // e.g. complexity score, line count
    public string? CodeSnippet { get; set; }

    public AnalysisReport Report { get; set; } = null!;
}
