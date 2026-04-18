namespace CodeInsight.Core.DTOs;

public class CodeIssueDto
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public string? MethodName { get; set; }
    public int? LineStart { get; set; }
    public int? LineEnd { get; set; }
    public int? MetricValue { get; set; }
    public string? CodeSnippet { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
