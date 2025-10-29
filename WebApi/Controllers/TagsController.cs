using Microsoft.AspNetCore.Mvc;
using PM.Application.Interfaces;
using PM.DTO;

namespace PM.API.Controllers;

/// <summary>
/// Handles CRUD operations for tags used in portfolios, holdings, or other entities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    /// <summary>
    /// Initializes a new instance of <see cref="TagsController"/>.
    /// </summary>
    /// <param name="tagService">Service for managing tags.</param>
    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    /// <param name="dto">Tag details.</param>
    /// <returns>Returns the created tag.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TagDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ModifyTagDTO dto)
    {
        var created = await _tagService.CreateAsync(dto.Name);
        return Ok(created);
    }

    /// <summary>
    /// Retrieves a tag by its ID.
    /// </summary>
    /// <param name="id">The ID of the tag.</param>
    /// <returns>Returns the tag if found, or 404 if not found.</returns>
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
    /// <returns>Returns all tags.</returns>
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
    /// <param name="id">The ID of the tag to update.</param>
    /// <param name="dto">Updated tag details.</param>
    /// <returns>Returns 204 No Content if successful, or 404 if the tag does not exist.</returns>
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
    /// <param name="id">The ID of the tag to delete.</param>
    /// <returns>Returns 204 No Content if deleted, or 404 if the tag does not exist.</returns>
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
