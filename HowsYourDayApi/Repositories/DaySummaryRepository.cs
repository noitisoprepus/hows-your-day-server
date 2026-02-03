using HowsYourDayApi.Data;
using HowsYourDayApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HowsYourDayApi.Repositories
{
    public class DaySummaryRepository : IDaySummaryRepository
    {
        private readonly HowsYourDayAppDbContext _context;

        public DaySummaryRepository(HowsYourDayAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DaySummary>> GetAllAsync()
        {
            return await _context.DaySummaries.ToListAsync();
        }

        public async Task<DaySummary> GetByIdAsync(Guid id)
        {
            return await _context.DaySummaries
                .FirstOrDefaultAsync(summary => summary.Id == id)
                ?? throw new KeyNotFoundException($"Day summary with ID {id} not found.");
        }

        public async Task<DaySummary> GetByDateAsync(DateTime dateUtc)
        {
            return await _context.DaySummaries
                .FirstOrDefaultAsync(summary => summary.DateStampUtc.Date == dateUtc.Date)
                ?? throw new KeyNotFoundException($"Day summary for date {dateUtc:yyyy-MM-dd} not found.");
        }

        public async Task<IEnumerable<DaySummary>> SearchAsync(DateTime fromUtc, DateTime toUtc)
        {
            return await _context.DaySummaries
                .Where(summary => summary.DateStampUtc >= fromUtc && 
                                    summary.DateStampUtc <= toUtc)
                .ToListAsync();
        }

        public async Task InsertAsync(DaySummary summary)
        {
            _context.DaySummaries.Add(summary);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DaySummary summary)
        {
            _context.DaySummaries.Update(summary);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(DaySummary summary)
        {
            _context.DaySummaries.Remove(summary);

            await _context.SaveChangesAsync();
        }
    }
}
