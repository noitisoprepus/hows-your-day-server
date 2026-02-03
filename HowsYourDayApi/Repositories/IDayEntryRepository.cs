using HowsYourDayApi.Models;

namespace HowsYourDayApi.Repositories
{
    public interface IDayEntryRepository
    {
        Task<IEnumerable<DayEntry>> GetAllAsync();
        Task<DayEntry> GetByIdAsync(Guid id);
        Task<IEnumerable<DayEntry>> SearchAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null);
        Task InsertAsync(DayEntry day);
        Task UpdateAsync(DayEntry day);
        Task DeleteAsync(DayEntry day);
    }
}
