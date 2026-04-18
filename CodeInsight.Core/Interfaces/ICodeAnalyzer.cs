using CodeInsight.Core.Entities;

namespace CodeInsight.Core.Interfaces;

public interface ICodeAnalyzer
{
    Task<List<CodeIssue>> AnalyzeFileAsync(string filePath, int reportId, AnalysisOptions options);
}

public class AnalysisOptions
{
    public int LongMethodThreshold { get; set; } = 30;
    public int ComplexityThreshold { get; set; } = 10;
    public int NestingThreshold { get; set; } = 4;
}
