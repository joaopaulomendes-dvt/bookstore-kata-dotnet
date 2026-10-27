namespace BookstoreRefactor.Infrastructure;

public sealed class GitHubMcpOptions
{
    public const string SectionName = "GitHubMcp";

    public string BaseUrl { get; set; } = "https://api.github.com";
    public string McpServerUrl { get; set; } = "https://api.githubcopilot.com/mcp/";
    public string RepositoryOwner { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string Toolset { get; set; } = "issues";
    public string? ApiToken { get; set; }
    public string UserAgent { get; set; } = "BookstoreRefactor";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(RepositoryOwner) &&
        !string.IsNullOrWhiteSpace(RepositoryName);
}
