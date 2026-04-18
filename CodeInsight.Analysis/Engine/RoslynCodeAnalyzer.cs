using Microsoft.CodeAnalysis.CSharp;
using CodeInsight.Core.Entities;
using CodeInsight.Core.Interfaces;
using CodeInsight.Analysis.Analyzers;
using Microsoft.Extensions.Logging;

namespace CodeInsight.Analysis.Engine;

/// <summary>
/// Orchestrates all Roslyn-based analyzers for a single C# file.
/// </summary>
public class RoslynCodeAnalyzer : ICodeAnalyzer
{
    private readonly ILogger<RoslynCodeAnalyzer> _logger;

    public RoslynCodeAnalyzer(ILogger<RoslynCodeAnalyzer> logger)
    {
        _logger = logger;
    }

    public async Task<List<CodeIssue>> AnalyzeFileAsync(string filePath, int reportId, AnalysisOptions options)
    {
        var issues = new List<CodeIssue>();

        try
        {
            var sourceCode = await File.ReadAllTextAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            var tree = CSharpSyntaxTree.ParseText(sourceCode);

            var complexityAnalyzer = new CyclomaticComplexityAnalyzer(options.ComplexityThreshold);
            var longMethodAnalyzer = new LongMethodAnalyzer(options.LongMethodThreshold);
            var nestingAnalyzer = new DeepNestingAnalyzer(options.NestingThreshold);
            var namingAnalyzer = new NamingConventionAnalyzer();

            issues.AddRange(complexityAnalyzer.Analyze(tree, filePath, fileName, reportId));
            issues.AddRange(longMethodAnalyzer.Analyze(tree, filePath, fileName, reportId));
            issues.AddRange(nestingAnalyzer.Analyze(tree, filePath, fileName, reportId));
            issues.AddRange(namingAnalyzer.Analyze(tree, filePath, fileName, reportId));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to analyze file: {FilePath}", filePath);
        }

        return issues;
    }
}
