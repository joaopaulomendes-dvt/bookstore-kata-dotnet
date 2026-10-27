using BookstoreRefactor.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreRefactor.Api.Controllers;

[ApiController]
[Route("api/github-mcp")]
public sealed class GitHubMcpController : ControllerBase
{
    private readonly IGitHubIssueClient _gitHubIssueClient;

    public GitHubMcpController(IGitHubIssueClient gitHubIssueClient)
    {
        _gitHubIssueClient = gitHubIssueClient;
    }

    [HttpGet("health")]
    public ActionResult<object> GetHealth()
    {
        return Ok(new
        {
            configured = _gitHubIssueClient.IsConfigured,
            toolset = "issues",
            repository = "joaopaulomendes-dvt/bookstore-kata-dotnet"
        });
    }

    [HttpGet("issues")]
    public async Task<ActionResult<IReadOnlyList<GitHubIssueSummary>>> GetOpenIssues(CancellationToken cancellationToken)
    {
        var issues = await _gitHubIssueClient.ListOpenIssuesAsync(cancellationToken);
        return Ok(issues);
    }

    [HttpGet("issues/{issueNumber:int}")]
    public async Task<ActionResult<GitHubIssueSummary>> GetIssue(int issueNumber, CancellationToken cancellationToken)
    {
        var issue = await _gitHubIssueClient.GetIssueAsync(issueNumber, cancellationToken);
        return issue is null ? NotFound() : Ok(issue);
    }
}
