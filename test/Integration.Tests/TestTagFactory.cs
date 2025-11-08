using PM.DTO;

namespace PM.Integration.Tests;

public static class TestTagFactory
{
    public static TagDTO CreateTagDto(int id = 1, string? name = "TestTag")
        => new TagDTO { Id = id, Name = name ?? "TestTag" };

    public static ModifyTagDTO CreateModifyDto(string? name = "NewTag")
        => new ModifyTagDTO { Name = name ?? "NewTag" };
}
