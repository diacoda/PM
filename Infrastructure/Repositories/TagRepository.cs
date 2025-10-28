using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Infrastructure.Data;

namespace PM.Infrastructure.Repositories;
public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tag> CreateAsync(Tag tag)
    {
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await _context.Tags.FindAsync(id);
    }

    public async Task<IEnumerable<Tag>> ListAsync()
    {
        return await _context.Tags.ToListAsync();
    }

    public async Task<bool> UpdateAsync(Tag updatedTag)
    {
        var existingTag = await _context.Tags.FindAsync(updatedTag.Id);
        if (existingTag is null) return false;

        existingTag.UpdateName(updatedTag.Name); // or set Name directly if no method
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return false;
        _context.Tags.Remove(tag);
        return await _context.SaveChangesAsync() > 0;
    }
}