using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Services
{
    public interface IDayEntryService
    {
        Task<IEnumerable<DayEntry>> GetDayEntriesAsync(DateTime? fromUtc = null, DateTime? toUtc = null);
        Task<DayEntry?> GetDayEntryAsync(Guid dayEntryId);
        
        Task<IEnumerable<DayEntry>> GetDayEntriesOfUserAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null);
        Task<DayEntry?> GetDayEntryOfUserTodayAsync(Guid userId);
        Task<bool> HasUserPostedTodayAsync(Guid userId);
        
        Task InsertDayEntryOfUserAsync(Guid userId, DayEntry entry);
        Task UpdateDayEntryOfUserAsync(Guid userId, DayEntry entry);
        Task DeleteDayEntriesOfUserAsync(Guid userId);
    }
}
