using BookstoreRefactor.Application;
using Microsoft.AspNetCore.Mvc;

namespace BookstoreRefactor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
    private readonly CategoryApplicationService _categoryApplicationService;

    public CategoriesController(CategoryApplicationService categoryApplicationService)
    {
        _categoryApplicationService = categoryApplicationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> Get(CancellationToken cancellationToken)
    {
        var categories = await _categoryApplicationService.ListAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoryResponse>> Post([FromBody] CategoryCreateRequest request, CancellationToken cancellationToken)
    {
        var created = await _categoryApplicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), created);
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> ToggleStatus(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryApplicationService.ToggleStatusAsync(id, cancellationToken);
        return Ok(category);
    }
}
