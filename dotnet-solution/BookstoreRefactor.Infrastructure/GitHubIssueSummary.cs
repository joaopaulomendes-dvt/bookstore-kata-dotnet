namespace BookstoreRefactor.Infrastructure;

public sealed record GitHubIssueSummary(
    int Number,
    string Title,
    string State,
    string Url,
    string? Body);
