using HowsYourDayApi.Data;
using HowsYourDayApi.Models;

namespace HowsYourDayApi.Repositories;

public class DayEntryAnalysisRepository : IDayEntryAnalysisRepository
{
    private readonly HowsYourDayAppDbContext _context;

    public DayEntryAnalysisRepository(HowsYourDayAppDbContext context)
    {
        _context = context;
    }

    public async Task<DayEntryAnalysis> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<DayEntryAnalysis>> SearchAsync(Guid? userId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        throw new NotImplementedException();
    }

    public async Task InsertAsync(DayEntryAnalysis analysis)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(DayEntryAnalysis analysis)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(DayEntryAnalysis analysis)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAllUserEntriesAsync(Guid userId)
    {
        throw new NotImplementedException();
    }
}