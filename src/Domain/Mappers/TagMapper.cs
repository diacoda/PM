using PM.Domain.Entities;
using PM.DTO;

namespace PM.Domain.Mappers;

public static class TagMapper
{
    public static TagDTO ToDTO(Tag tag)
    {
        return new TagDTO
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }

    public static Tag ToEntity(TagDTO dto)
    {
        return new Tag(dto.Name); // assumes constructor handles trimming
    }
}
