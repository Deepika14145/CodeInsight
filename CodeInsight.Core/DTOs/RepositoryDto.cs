namespace CodeInsight.Core.DTOs;

public class RepositoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public int? Stars { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ReportCount { get; set; }
}
