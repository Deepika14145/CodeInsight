namespace CodeInsight.Core.Entities;

public class Repository
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Owner { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public int? Stars { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AnalysisReport> Reports { get; set; } = new List<AnalysisReport>();
}
