using PM.Domain.Entities;

namespace PM.Application.Interfaces;

public interface ITagRepository
{
    Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Tag>> ListAsync(CancellationToken ct = default);
    Task<bool> UpdateAsync(Tag tag, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}