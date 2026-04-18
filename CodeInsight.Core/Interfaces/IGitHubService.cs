namespace CodeInsight.Core.Interfaces;

public interface IGitHubService
{
    Task<GitHubRepoMetadata?> GetRepositoryMetadataAsync(string repoUrl);
    Task<string> CloneRepositoryAsync(string repoUrl, string targetPath);
}

public class GitHubRepoMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Language { get; set; }
    public int Stars { get; set; }
    public string CloneUrl { get; set; } = string.Empty;
}
