using HowsYourDayApi.Models;

namespace HowsYourDayApi.Repositories;

public interface IDayEntryAnalysisRepository
{
    Task<DayEntryAnalysis> GetByIdAsync(Guid id);
    Task<IEnumerable<DayEntryAnalysis>> SearchAsync(Guid? userId = null, DateTime? fromUtc = null, DateTime? toUtc = null);
    Task InsertAsync(DayEntryAnalysis analysis);
    Task UpdateAsync(DayEntryAnalysis analysis);
    Task DeleteAsync(DayEntryAnalysis analysis);
    Task DeleteAllUserEntriesAsync(Guid userId);
}