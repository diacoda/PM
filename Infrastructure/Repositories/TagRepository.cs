using Microsoft.EntityFrameworkCore;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Infrastructure.Data;

namespace PM.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly PortfolioDbContext _context;

    public TagRepository(PortfolioDbContext context)
    {
        _context = context;
    }

    public async Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default)
    {
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Tags.FindAsync(id, ct);
    }

    public async Task<IEnumerable<Tag>> ListAsync(CancellationToken ct = default)
    {
        return await _context.Tags.ToListAsync(ct);
    }

    public async Task<bool> UpdateAsync(Tag updatedTag, CancellationToken ct = default)
    {
        var existingTag = await _context.Tags.FindAsync(updatedTag.Id, ct);
        if (existingTag is null) return false;

        existingTag.UpdateName(updatedTag.Name); // or set Name directly if no method
        return await _context.SaveChangesAsync(ct) > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var tag = await _context.Tags.FindAsync(id, ct);
        if (tag == null) return false;
        _context.Tags.Remove(tag);
        return await _context.SaveChangesAsync(ct) > 0;
    }
}