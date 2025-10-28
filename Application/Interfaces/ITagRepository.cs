using PM.Domain.Entities;

namespace PM.Application.Interfaces;
public interface ITagRepository
{
    Task<Tag> CreateAsync(Tag tag);
    Task<Tag?> GetByIdAsync(int id);
    Task<IEnumerable<Tag>> ListAsync();
    Task<bool> UpdateAsync(Tag tag);
    Task<bool> DeleteAsync(int id);
}