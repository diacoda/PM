using PM.DTO;

namespace PM.Application.Interfaces;
public interface ITagService
{
    Task<TagDTO> CreateAsync(string name, CancellationToken ct = default);
    Task<TagDTO?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TagDTO>> ListAsync(CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, string name, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}