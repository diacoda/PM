namespace model.Domain.Values;
public readonly record struct Tag(string Name)
{
    public override string ToString() => Name;

    public static Tag From(string name) => new Tag(name.Trim().ToLowerInvariant());
}