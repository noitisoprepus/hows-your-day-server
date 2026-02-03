using HowsYourDayApi.DTOs.Day;

namespace HowsYourDayApi.Services
{
    public interface IDayEntryService
    {
        Task<IEnumerable<DayEntryDto>> GetDayEntriesAsync();
        Task<DayEntryDto?> GetDayEntryAsync(Guid dayEntryId);
        Task<bool> HasUserPostedTodayAsync(Guid userId);
        Task<IEnumerable<DayEntryDto>> GetDayEntriesOfUserAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null);
        Task<DayEntryDto> GetDayEntryOfUserTodayAsync(Guid userId);
        Task InsertDayEntryOfUserAsync(Guid userId, CreateDayEntryDto day);
        Task UpdateDayEntryOfUserTodayAsync(Guid userId, CreateDayEntryDto day);
    }
}
