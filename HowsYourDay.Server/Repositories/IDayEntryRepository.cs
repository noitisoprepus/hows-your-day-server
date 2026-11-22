using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Repositories
{
    public interface IDayEntryRepository
    {
        Task<DayEntry?> GetByIdAsync(Guid id);
        Task<DayEntry?> GetByUserAndDateAsync(Guid userId, DateOnly entryDate);
        Task<IEnumerable<DayEntry>> SearchAsync(Guid? userId = null, DateTime? fromUtc = null, DateTime? toUtc = null);
        Task InsertAsync(DayEntry day);
        Task UpdateAsync(DayEntry day);
        Task DeleteAsync(DayEntry day);
        Task DeleteAllUserEntriesAsync(Guid userId);
    }
}
