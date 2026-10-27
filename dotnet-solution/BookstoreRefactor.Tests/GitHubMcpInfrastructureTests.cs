using BookstoreRefactor.Infrastructure;
using Xunit;

namespace BookstoreRefactor.Tests;

public class GitHubMcpInfrastructureTests
{
    [Fact]
    public void GitHubMcpOptions_WhenRepositoryValuesArePresent_IsConfigured()
    {
        var options = new GitHubMcpOptions
        {
            RepositoryOwner = "joaopaulomendes-dvt",
            RepositoryName = "bookstore-kata-dotnet",
            Toolset = "issues"
        };

        Assert.True(options.IsConfigured);
        Assert.Equal("issues", options.Toolset);
        Assert.Equal("joaopaulomendes-dvt", options.RepositoryOwner);
        Assert.Equal("bookstore-kata-dotnet", options.RepositoryName);
    }

    [Fact]
    public void GitHubIssueSummary_WhenCreated_ContainsExpectedDetails()
    {
        var issue = new GitHubIssueSummary(
            1,
            "FEAT-007: Author Support",
            "open",
            "https://github.com/joaopaulomendes-dvt/bookstore-kata-dotnet/issues/1",
            "Feature request");

        Assert.Equal(1, issue.Number);
        Assert.Equal("FEAT-007: Author Support", issue.Title);
        Assert.Equal("open", issue.State);
        Assert.Equal("https://github.com/joaopaulomendes-dvt/bookstore-kata-dotnet/issues/1", issue.Url);
    }
}
