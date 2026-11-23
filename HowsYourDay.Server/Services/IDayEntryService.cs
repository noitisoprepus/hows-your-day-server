using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Services
{
    public interface IDayEntryService
    {
        Task<(IReadOnlyList<DayEntry> Entries, bool HasMore)> GetDayEntriesOfUserAsync(
            Guid userId,
            DateOnly? before = null,
            int limit = 20,
            DateOnly? from = null,
            bool analyzedOnly = false);

        Task<DayEntry?> GetDayEntryOfUserTodayAsync(Guid userId);
        Task<DayEntry?> GetDayEntryOfUserByDateAsync(Guid userId, DateOnly entryDate);

        Task InsertDayEntryOfUserAsync(Guid userId, DayEntry entry, DateOnly? expectedDate = null);
        Task UpdateDayEntryOfUserAsync(Guid userId, DayEntry entry);
        Task DeleteDayEntriesOfUserAsync(Guid userId);
    }
}
