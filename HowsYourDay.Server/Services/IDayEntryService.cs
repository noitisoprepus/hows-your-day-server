using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Services
{
    public interface IDayEntryService
    {
        Task<(IReadOnlyList<DayEntry> Entries, bool HasMore)> GetDayEntriesOfUserAsync(
            Guid userId,
            DateOnly? before = null,
            int limit = 20);

        Task<DayEntry?> GetDayEntryOfUserTodayAsync(Guid userId);
        Task<DayEntry?> GetDayEntryOfUserByDateAsync(Guid userId, DateOnly entryDate);

        Task InsertDayEntryOfUserAsync(Guid userId, DayEntry entry);
        Task UpdateDayEntryOfUserAsync(Guid userId, DayEntry entry);
        Task DeleteDayEntriesOfUserAsync(Guid userId);
    }
}
