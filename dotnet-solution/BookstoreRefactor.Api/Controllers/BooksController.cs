using BookstoreRefactor.Application;
using BookstoreRefactor.Domain;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreRefactor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BooksController : ControllerBase
{
    private readonly BookApplicationService _bookApplicationService;

    public BooksController(BookApplicationService bookApplicationService)
    {
        _bookApplicationService = bookApplicationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BookResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BookResponse>>> Get(
        [FromQuery] Guid? category,
        [FromQuery] string? search,
        [FromQuery] BookStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = new BookListQuery(category, search, status);
        var books = await _bookApplicationService.ListAsync(query, cancellationToken);
        return Ok(books);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var book = await _bookApplicationService.GetByIdAsync(id, cancellationToken);
        return Ok(book);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookResponse>> Post([FromBody] BookCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _bookApplicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponse>> Put(Guid id, [FromBody] BookUpdateRequest request, CancellationToken cancellationToken)
    {
        var payload = request with { Id = id };
        var updated = await _bookApplicationService.UpdateAsync(payload, cancellationToken);
        return Ok(updated);
    }
}
