using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeInsight.Core.Entities;
using System.Text.RegularExpressions;

namespace CodeInsight.Analysis.Analyzers;

/// <summary>
/// Detects naming convention violations: single-letter variables, non-PascalCase methods, etc.
/// </summary>
public class NamingConventionAnalyzer
{
    private static readonly Regex PascalCase = new(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);
    private static readonly Regex CamelCase = new(@"^[a-z][a-zA-Z0-9]*$", RegexOptions.Compiled);
    private static readonly string[] CommonBadNames = { "temp", "tmp", "foo", "bar", "baz", "x", "y", "z", "a", "b", "c", "data2", "obj", "obj2" };

    public List<CodeIssue> Analyze(SyntaxTree tree, string filePath, string fileName, int reportId)
    {
        var issues = new List<CodeIssue>();
        var root = tree.GetRoot();

        // Check method names (should be PascalCase)
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            var name = method.Identifier.Text;
            if (!PascalCase.IsMatch(name))
            {
                var line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                issues.Add(CreateIssue(reportId, filePath, fileName,
                    $"Method '{name}' does not follow PascalCase naming convention.",
                    "Rename the method to PascalCase (e.g., 'GetUserData' instead of 'getUserData' or 'get_user_data').",
                    name, line));
            }
        }

        // Check local variable names for bad patterns
        foreach (var variable in root.DescendantNodes().OfType<VariableDeclaratorSyntax>())
        {
            var name = variable.Identifier.Text;
            if (CommonBadNames.Contains(name.ToLower()) || (name.Length == 1 && name != "_"))
            {
                var line = variable.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                issues.Add(CreateIssue(reportId, filePath, fileName,
                    $"Variable '{name}' has a non-descriptive name.",
                    "Use meaningful, descriptive variable names that convey intent (e.g., 'userCount' instead of 'x').",
                    null, line));
            }
        }

        return issues;
    }

    private static CodeIssue CreateIssue(int reportId, string filePath, string fileName,
        string description, string suggestion, string? methodName, int line) => new()
    {
        ReportId = reportId,
        FilePath = filePath,
        FileName = fileName,
        IssueType = "NamingConvention",
        Severity = "Low",
        Description = description,
        Suggestion = suggestion,
        MethodName = methodName,
        LineStart = line,
        LineEnd = line
    };
}
