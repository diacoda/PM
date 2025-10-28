using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;

namespace PM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TagDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ModifyTagDTO dto)
    {
        var created = await _tagService.CreateAsync(dto.Name);
        return Ok(created);
    }

    /// <summary>
    /// Retrieves a tag by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TagDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var tag = await _tagService.GetByIdAsync(id);
        if (tag is null) return NotFound();
        return Ok(tag);
    }

    /// <summary>
    /// Lists all tags.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TagDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List()
    {
        var tags = await _tagService.ListAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Updates a tag by ID.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ModifyTagDTO dto)
    {
        var success = await _tagService.UpdateAsync(id, dto.Name);
        if (!success) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Deletes a tag by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _tagService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}