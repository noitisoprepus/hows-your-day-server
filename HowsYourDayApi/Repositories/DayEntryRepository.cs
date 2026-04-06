using HowsYourDayApi.Data;
using HowsYourDayApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HowsYourDayApi.Repositories
{
    public class DayEntryRepository : IDayEntryRepository
    {
        private readonly HowsYourDayAppDbContext _context;

        public DayEntryRepository(HowsYourDayAppDbContext context)
        {
            _context = context;
        }

        public async Task<DayEntry?> GetByIdAsync(Guid id)
        {
            return await _context.DayEntries
                .FirstOrDefaultAsync(day => day.Id == id);
        }

        public async Task<IEnumerable<DayEntry>> SearchAsync(Guid? userId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            return await _context.DayEntries
                .Where(day => (!userId.HasValue || day.UserId == userId) &&
                              (!fromUtc.HasValue || day.LoggedAtUtc >= fromUtc) &&
                              (!toUtc.HasValue || day.LoggedAtUtc <= toUtc))
                .ToListAsync();
        }

        public async Task InsertAsync(DayEntry day)
        {
            _context.DayEntries.Add(day);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DayEntry day)
        {
            _context.DayEntries.Update(day);

            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(DayEntry day)
        {
            _context.DayEntries.Remove(day);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllUserEntriesAsync(Guid userId)
        {
            _context.DayEntries.RemoveRange(await _context.DayEntries.Where(day => day.UserId == userId).ToListAsync());

            await _context.SaveChangesAsync();
        }
    }
}
