using HowsYourDay.Server.Data;
using HowsYourDay.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace HowsYourDay.Server.Repositories
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

        public async Task<DayEntry?> GetByUserAndDateAsync(Guid userId, DateOnly entryDate)
        {
            return await _context.DayEntries
                .Include(day => day.Analysis)
                .FirstOrDefaultAsync(day => day.UserId == userId && day.EntryDate == entryDate);
        }

        public async Task<IReadOnlyList<DayEntry>> GetPageByUserAsync(Guid userId, DateOnly? before, int take)
        {
            return await _context.DayEntries
                .Where(day => day.UserId == userId &&
                              (!before.HasValue || day.EntryDate < before.Value))
                .Include(day => day.Analysis)
                .OrderByDescending(day => day.EntryDate)
                .Take(take)
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
            var userEntries = await _context.DayEntries
                .Where(day => day.UserId == userId)
                .ToListAsync();

            _context.DayEntries.RemoveRange(userEntries);

            await _context.SaveChangesAsync();
        }
    }
}
