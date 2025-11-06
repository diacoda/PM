namespace PM.DTO;

/// <summary>
/// Data Transfer Object representing a tag.
/// </summary>
public class TagDTO
{
    /// <summary>
    /// Unique identifier of the tag.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the tag.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
