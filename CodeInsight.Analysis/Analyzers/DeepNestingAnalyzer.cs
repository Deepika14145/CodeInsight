using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeInsight.Core.Entities;

namespace CodeInsight.Analysis.Analyzers;

/// <summary>
/// Detects deeply nested code blocks that hurt readability.
/// </summary>
public class DeepNestingAnalyzer
{
    private readonly int _threshold;

    public DeepNestingAnalyzer(int threshold = 4)
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
            int maxDepth = GetMaxNestingDepth(method.Body ?? (SyntaxNode?)method.ExpressionBody ?? method, 0);
            if (maxDepth >= _threshold)
            {
                var lineSpan = method.GetLocation().GetLineSpan();
                issues.Add(new CodeIssue
                {
                    ReportId = reportId,
                    FilePath = filePath,
                    FileName = fileName,
                    IssueType = "DeepNesting",
                    Severity = maxDepth >= 6 ? "High" : "Medium",
                    Description = $"Method '{method.Identifier.Text}' has nesting depth of {maxDepth} (threshold: {_threshold}).",
                    Suggestion = "Use early returns (guard clauses) to reduce nesting. Extract nested blocks into separate methods. Consider inverting conditions to fail fast.",
                    MethodName = method.Identifier.Text,
                    LineStart = lineSpan.StartLinePosition.Line + 1,
                    LineEnd = lineSpan.EndLinePosition.Line + 1,
                    MetricValue = maxDepth,
                    CodeSnippet = GetSnippet(method)
                });
            }
        }

        return issues;
    }

    private static int GetMaxNestingDepth(SyntaxNode node, int currentDepth)
    {
        int max = currentDepth;
        foreach (var child in node.ChildNodes())
        {
            bool isNestingNode = child is IfStatementSyntax
                or WhileStatementSyntax
                or ForStatementSyntax
                or ForEachStatementSyntax
                or SwitchStatementSyntax
                or TryStatementSyntax
                or BlockSyntax { Parent: not (MethodDeclarationSyntax or BaseMethodDeclarationSyntax) };

            int childDepth = isNestingNode
                ? GetMaxNestingDepth(child, currentDepth + 1)
                : GetMaxNestingDepth(child, currentDepth);

            if (childDepth > max) max = childDepth;
        }
        return max;
    }

    private static string GetSnippet(SyntaxNode node)
    {
        var lines = node.ToFullString().Split('\n').Take(8);
        return string.Join('\n', lines) + "\n...";
    }
}
