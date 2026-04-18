using System.ComponentModel.DataAnnotations;

namespace CodeInsight.Core.DTOs;

public class AnalyzeRequestDto
{
    [Required]
    [Url]
    public string RepositoryUrl { get; set; } = string.Empty;

    public int LongMethodThreshold { get; set; } = 30;
    public int ComplexityThreshold { get; set; } = 10;
    public int NestingThreshold { get; set; } = 4;
}
