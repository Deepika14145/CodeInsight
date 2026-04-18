using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeInsight.Core.Entities;

namespace CodeInsight.Analysis.Analyzers;

/// <summary>
/// Detects duplicate code blocks across methods using normalized token hashing.
/// </summary>
public class DuplicateCodeAnalyzer
{
    private const int MinBlockLines = 6;

    // Shared state across files for cross-file duplicate detection
    private readonly Dictionary<string, (string FilePath, string FileName, string MethodName, int LineStart)> _seenBlocks;

    public DuplicateCodeAnalyzer(Dictionary<string, (string, string, string, int)>? sharedState = null)
    {
        _seenBlocks = sharedState ?? new Dictionary<string, (string, string, string, int)>();
    }

    public List<CodeIssue> Analyze(SyntaxTree tree, string filePath, string fileName, int reportId)
    {
        var issues = new List<CodeIssue>();
        var root = tree.GetRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        foreach (var method in methods)
        {
            if (method.Body == null) continue;

            var statements = method.Body.Statements;
            if (statements.Count < MinBlockLines) continue;

            // Slide a window of MinBlockLines statements
            for (int i = 0; i <= statements.Count - MinBlockLines; i++)
            {
                var block = statements.Skip(i).Take(MinBlockLines).ToList();
                var normalized = NormalizeBlock(block);
                var hash = ComputeHash(normalized);

                var lineSpan = block[0].GetLocation().GetLineSpan();
                int lineStart = lineSpan.StartLinePosition.Line + 1;

                if (_seenBlocks.TryGetValue(hash, out var original))
                {
                    // Only report if it's a different location
                    if (original.FilePath != filePath || original.LineStart != lineStart)
                    {
                        issues.Add(new CodeIssue
                        {
                            ReportId = reportId,
                            FilePath = filePath,
                            FileName = fileName,
                            IssueType = "DuplicateCode",
                            Severity = "Medium",
                            Description = $"Duplicate code block found in '{method.Identifier.Text}' (also in '{original.FileName}' → '{original.MethodName}' at line {original.LineStart}).",
                            Suggestion = "Extract the duplicated logic into a shared method or utility class. Apply the DRY (Don't Repeat Yourself) principle.",
                            MethodName = method.Identifier.Text,
                            LineStart = lineStart,
                            LineEnd = lineStart + MinBlockLines - 1,
                            MetricValue = MinBlockLines,
                            CodeSnippet = string.Join('\n', block.Take(4).Select(s => s.ToFullString().Trim())) + "\n..."
                        });
                    }
                }
                else
                {
                    _seenBlocks[hash] = (filePath, fileName, method.Identifier.Text, lineStart);
                }
            }
        }

        return issues;
    }

    private static string NormalizeBlock(IEnumerable<StatementSyntax> statements)
    {
        // Normalize identifiers to detect structural duplicates
        var tokens = statements
            .SelectMany(s => s.DescendantTokens())
            .Select(t => t.IsKind(SyntaxKind.IdentifierToken) ? "ID" : t.ValueText);
        return string.Join(" ", tokens);
    }

    private static string ComputeHash(string input)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}
