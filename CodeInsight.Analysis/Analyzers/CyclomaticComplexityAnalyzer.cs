using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeInsight.Core.Entities;
using CodeInsight.Core.Interfaces;

namespace CodeInsight.Analysis.Analyzers;

/// <summary>
/// Calculates cyclomatic complexity per method using Roslyn.
/// Complexity = 1 + number of branching points (if, else if, while, for, foreach, case, catch, &&, ||, ??)
/// </summary>
public class CyclomaticComplexityAnalyzer
{
    private readonly int _threshold;

    public CyclomaticComplexityAnalyzer(int threshold = 10)
    {
        _threshold = threshold;
    }

    public List<CodeIssue> Analyze(SyntaxTree tree, string filePath, string fileName, int reportId)
    {
        var issues = new List<CodeIssue>();
        var root = tree.GetRoot();
        var methods = root.DescendantNodes()
            .OfType<BaseMethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            var complexity = CalculateComplexity(method);
            if (complexity > _threshold)
            {
                var methodName = GetMethodName(method);
                var lineSpan = method.GetLocation().GetLineSpan();
                var snippet = GetSnippet(method);

                issues.Add(new CodeIssue
                {
                    ReportId = reportId,
                    FilePath = filePath,
                    FileName = fileName,
                    IssueType = "CyclomaticComplexity",
                    Severity = complexity > 20 ? "Critical" : complexity > 15 ? "High" : "Medium",
                    Description = $"Method '{methodName}' has cyclomatic complexity of {complexity} (threshold: {_threshold}).",
                    Suggestion = "Break this method into smaller, focused methods. Each method should do one thing. Consider using the Strategy or Command pattern to reduce branching.",
                    MethodName = methodName,
                    LineStart = lineSpan.StartLinePosition.Line + 1,
                    LineEnd = lineSpan.EndLinePosition.Line + 1,
                    MetricValue = complexity,
                    CodeSnippet = snippet
                });
            }
        }

        return issues;
    }

    private static int CalculateComplexity(SyntaxNode method)
    {
        int complexity = 1;
        foreach (var node in method.DescendantNodes())
        {
            complexity += node switch
            {
                IfStatementSyntax => 1,
                ElseClauseSyntax => 1,
                WhileStatementSyntax => 1,
                ForStatementSyntax => 1,
                ForEachStatementSyntax => 1,
                SwitchSectionSyntax => 1,
                CatchClauseSyntax => 1,
                ConditionalExpressionSyntax => 1,
                BinaryExpressionSyntax b when
                    b.IsKind(SyntaxKind.LogicalAndExpression) ||
                    b.IsKind(SyntaxKind.LogicalOrExpression) => 1,
                AssignmentExpressionSyntax a when
                    a.IsKind(SyntaxKind.CoalesceAssignmentExpression) => 1,
                BinaryExpressionSyntax b2 when
                    b2.IsKind(SyntaxKind.CoalesceExpression) => 1,
                _ => 0
            };
        }
        return complexity;
    }

    private static string GetMethodName(BaseMethodDeclarationSyntax method) => method switch
    {
        MethodDeclarationSyntax m => m.Identifier.Text,
        ConstructorDeclarationSyntax c => c.Identifier.Text + " (constructor)",
        _ => "Unknown"
    };

    private static string GetSnippet(SyntaxNode node)
    {
        var text = node.ToFullString();
        var lines = text.Split('\n').Take(10);
        return string.Join('\n', lines) + (text.Split('\n').Length > 10 ? "\n..." : "");
    }
}
