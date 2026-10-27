using BookstoreRefactor.Application;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreRefactor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthorsController : ControllerBase
{
    private readonly AuthorApplicationService _authorApplicationService;

    public AuthorsController(AuthorApplicationService authorApplicationService)
    {
        _authorApplicationService = authorApplicationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuthorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuthorResponse>>> Get(CancellationToken cancellationToken)
    {
        var authors = await _authorApplicationService.ListAsync(cancellationToken);
        return Ok(authors);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthorResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var author = await _authorApplicationService.GetByIdAsync(id, cancellationToken);
        return Ok(author);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthorResponse>> Post([FromBody] AuthorCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _authorApplicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthorResponse>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _authorApplicationService.DeleteAsync(id, cancellationToken);
        return Ok(deleted);
    }
}
