using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace BookstoreRefactor.Infrastructure;

public sealed class GitHubIssueClient : IGitHubIssueClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly GitHubMcpOptions _options;

    public GitHubIssueClient(HttpClient httpClient, IOptions<GitHubMcpOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        var baseAddress = string.IsNullOrWhiteSpace(_options.BaseUrl)
            ? "https://api.github.com"
            : _options.BaseUrl.TrimEnd('/');

        _httpClient.BaseAddress = new Uri(baseAddress + "/");
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(string.IsNullOrWhiteSpace(_options.UserAgent) ? "BookstoreRefactor" : _options.UserAgent);

        if (!string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken.Trim());
        }
    }

    public bool IsConfigured => _options.IsConfigured;

    public async Task<IReadOnlyList<GitHubIssueSummary>> ListOpenIssuesAsync(CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync(
            $"repos/{_options.RepositoryOwner}/{_options.RepositoryName}/issues?state=open&per_page=100",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var issues = JsonSerializer.Deserialize<List<GitHubIssueDocument>>(json, JsonOptions) ?? [];

        return issues
            .Where(issue => issue.PullRequest is null)
            .Select(issue => new GitHubIssueSummary(
                issue.Number,
                issue.Title,
                issue.State,
                issue.HtmlUrl,
                issue.Body))
            .ToList();
    }

    public async Task<GitHubIssueSummary?> GetIssueAsync(int issueNumber, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        using var response = await _httpClient.GetAsync(
            $"repos/{_options.RepositoryOwner}/{_options.RepositoryName}/issues/{issueNumber}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var issue = JsonSerializer.Deserialize<GitHubIssueDocument>(json, JsonOptions);

        if (issue is null)
        {
            return null;
        }

        return new GitHubIssueSummary(
            issue.Number,
            issue.Title,
            issue.State,
            issue.HtmlUrl,
            issue.Body);
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "GitHub MCP is not configured. Set GitHubMcp:RepositoryOwner and GitHubMcp:RepositoryName in configuration.");
        }
    }

    private sealed class GitHubIssueDocument
    {
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string? Body { get; set; }
        public GitHubPullRequestDocument? PullRequest { get; set; }
    }

    private sealed class GitHubPullRequestDocument
    {
        public string Url { get; set; } = string.Empty;
    }
}
