using PM.DTO;

namespace PM.Application.Interfaces;
public interface ITagService
{
    Task<TagDTO> CreateAsync(string name);
    Task<TagDTO?> GetByIdAsync(int id);
    Task<IEnumerable<TagDTO>> ListAsync();
    Task<bool> UpdateAsync(int id, string name);
    Task<bool> DeleteAsync(int id);
}