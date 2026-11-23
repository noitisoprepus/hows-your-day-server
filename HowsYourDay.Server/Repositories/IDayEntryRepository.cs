using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Repositories
{
    public interface IDayEntryRepository
    {
        Task<DayEntry?> GetByIdAsync(Guid id);
        Task<DayEntry?> GetByUserAndDateAsync(Guid userId, DateOnly entryDate);
        Task<IReadOnlyList<DayEntry>> GetPageByUserAsync(Guid userId, DateOnly? before, int take);
        Task InsertAsync(DayEntry day);
        Task UpdateAsync(DayEntry day);
        Task DeleteAsync(DayEntry day);
        Task DeleteAllUserEntriesAsync(Guid userId);
    }
}
