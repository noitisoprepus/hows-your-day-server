using HowsYourDayApi.Data;
using HowsYourDayApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HowsYourDayApi.Repositories;

public class DayEntryAnalysisRepository : IDayEntryAnalysisRepository
{
    private readonly HowsYourDayAppDbContext _context;

    public DayEntryAnalysisRepository(HowsYourDayAppDbContext context)
    {
        _context = context;
    }

    public async Task<DayEntryAnalysis?> GetByIdAsync(Guid id)
    {
        return await _context.DayEntryAnalyses
            .FirstOrDefaultAsync(analysis => analysis.Id == id);
    }

    public async Task<IEnumerable<DayEntryAnalysis>> SearchAsync(Guid? userId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        return await _context.DayEntryAnalyses
            .Where(analysis => (!userId.HasValue || analysis.UserId == userId) &&
                               (!fromUtc.HasValue || analysis.AnalyzedAtUtc >= fromUtc) &&
                               (!toUtc.HasValue || analysis.AnalyzedAtUtc <= toUtc))
            .ToListAsync();
    }

    public async Task InsertAsync(DayEntryAnalysis analysis)
    {
        _context.DayEntryAnalyses.Add(analysis);
        
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DayEntryAnalysis analysis)
    {
        _context.DayEntryAnalyses.Update(analysis);
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(DayEntryAnalysis analysis)
    {
        _context.DayEntryAnalyses.Remove(analysis);
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAllUserEntriesAsync(Guid userId)
    {
        var userEntries = await SearchAsync(userId);
        _context.DayEntryAnalyses.RemoveRange(userEntries);
        
        await _context.SaveChangesAsync();
    }
}