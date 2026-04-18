using CodeInsight.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;
using System.Text.RegularExpressions;

namespace CodeInsight.Infrastructure.Services;

public class GitHubService : IGitHubService
{
    private readonly ILogger<GitHubService> _logger;
    private readonly string? _githubToken;

    public GitHubService(ILogger<GitHubService> logger, IConfiguration config)
    {
        _logger = logger;
        _githubToken = config["GitHub:Token"];
    }

    public async Task<GitHubRepoMetadata?> GetRepositoryMetadataAsync(string repoUrl)
    {
        try
        {
            var (owner, repo) = ParseGitHubUrl(repoUrl);
            var client = CreateClient();
            var repository = await client.Repository.Get(owner, repo);

            return new GitHubRepoMetadata
            {
                Name = repository.Name,
                Owner = repository.Owner.Login,
                Description = repository.Description,
                Language = repository.Language,
                Stars = repository.StargazersCount,
                CloneUrl = repository.CloneUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch GitHub metadata for {Url}", repoUrl);
            return null;
        }
    }

    public async Task<string> CloneRepositoryAsync(string repoUrl, string targetPath)
    {
        _logger.LogInformation("Cloning repository {Url} to {Path}", repoUrl, targetPath);

        // Use git CLI for cloning (LibGit2Sharp has native lib issues on some platforms)
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"clone --depth 1 \"{repoUrl}\" \"{targetPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Git clone failed: {error}");
        }

        return targetPath;
    }

    private static (string owner, string repo) ParseGitHubUrl(string url)
    {
        var match = Regex.Match(url, @"github\.com[/:]([^/]+)/([^/\.]+)");
        if (!match.Success)
            throw new ArgumentException($"Invalid GitHub URL: {url}");
        return (match.Groups[1].Value, match.Groups[2].Value.Replace(".git", ""));
    }

    private GitHubClient CreateClient()
    {
        var client = new GitHubClient(new ProductHeaderValue("CodeInsight"));
        if (!string.IsNullOrEmpty(_githubToken))
            client.Credentials = new Credentials(_githubToken);
        return client;
    }
}
