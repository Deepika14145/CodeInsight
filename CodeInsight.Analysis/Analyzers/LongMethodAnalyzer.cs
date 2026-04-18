using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeInsight.Core.Entities;

namespace CodeInsight.Analysis.Analyzers;

/// <summary>
/// Detects methods that exceed a configurable line count threshold.
/// </summary>
public class LongMethodAnalyzer
{
    private readonly int _threshold;

    public LongMethodAnalyzer(int threshold = 30)
    {
        _threshold = threshold;
    }

    public List<CodeIssue> Analyze(SyntaxTree tree, string filePath, string fileName, int reportId)
    {
        var issues = new List<CodeIssue>();
        var root = tree.GetRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            var lineSpan = method.GetLocation().GetLineSpan();
            int startLine = lineSpan.StartLinePosition.Line + 1;
            int endLine = lineSpan.EndLinePosition.Line + 1;
            int lineCount = endLine - startLine + 1;

            if (lineCount > _threshold)
            {
                var methodName = method.Identifier.Text;
                issues.Add(new CodeIssue
                {
                    ReportId = reportId,
                    FilePath = filePath,
                    FileName = fileName,
                    IssueType = "LongMethod",
                    Severity = lineCount > 100 ? "Critical" : lineCount > 60 ? "High" : "Medium",
                    Description = $"Method '{methodName}' is {lineCount} lines long (threshold: {_threshold}).",
                    Suggestion = "Extract logical blocks into separate private methods. Apply the Single Responsibility Principle — each method should have one clear purpose.",
                    MethodName = methodName,
                    LineStart = startLine,
                    LineEnd = endLine,
                    MetricValue = lineCount,
                    CodeSnippet = GetSnippet(method)
                });
            }
        }

        return issues;
    }

    private static string GetSnippet(SyntaxNode node)
    {
        var lines = node.ToFullString().Split('\n').Take(8);
        return string.Join('\n', lines) + "\n...";
    }
}
