namespace PM.DTO;

/// <summary>
/// Data Transfer Object used to create or update a tag.
/// </summary>
public class ModifyTagDTO
{
    /// <summary>
    /// Name of the tag.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
