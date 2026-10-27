namespace BookstoreRefactor.Infrastructure;

public interface IGitHubIssueClient
{
    bool IsConfigured { get; }

    Task<IReadOnlyList<GitHubIssueSummary>> ListOpenIssuesAsync(CancellationToken cancellationToken = default);

    Task<GitHubIssueSummary?> GetIssueAsync(int issueNumber, CancellationToken cancellationToken = default);
}
