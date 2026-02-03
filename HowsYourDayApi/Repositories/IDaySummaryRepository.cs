using HowsYourDayApi.Models;

namespace HowsYourDayApi.Repositories
{
    public interface IDaySummaryRepository
    {
        Task<IEnumerable<DaySummary>> GetAllAsync();
        Task<DaySummary> GetByIdAsync(Guid id);
        Task<DaySummary> GetByDateAsync(DateTime dateUtc);
        Task<IEnumerable<DaySummary>> SearchAsync(DateTime fromUtc, DateTime toUtc);
        Task InsertAsync(DaySummary summary);
        Task UpdateAsync(DaySummary summary);
        Task DeleteAsync(DaySummary summary);
    }
}
