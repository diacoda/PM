using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Mappers;
using PM.DTO;

namespace PM.Application.Services;
public class TagService : ITagService
{
    private readonly ITagRepository _repo;

    public TagService(ITagRepository repo)
    {
        _repo = repo;
    }

    public async Task<TagDTO> CreateAsync(string name)
    {
        var tag = new Tag(name);
        var created = await _repo.CreateAsync(tag);
        return TagMapper.ToDTO(created);
    }

    public async Task<TagDTO?> GetByIdAsync(int id)
    {
        var tag = await _repo.GetByIdAsync(id);
        return tag is null ? null : TagMapper.ToDTO(tag);
    }

    public async Task<IEnumerable<TagDTO>> ListAsync()
    {
        var tags = await _repo.ListAsync();
        return tags.Select(TagMapper.ToDTO);
    }

    public async Task<bool> UpdateAsync(int id, string name)
    {
        var tag = await _repo.GetByIdAsync(id);
        if (tag is null) return false;
        tag = new Tag(name) { Id = id }; // recreate with updated name
        return await _repo.UpdateAsync(tag);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repo.DeleteAsync(id);
    }
}