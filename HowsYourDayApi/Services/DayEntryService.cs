using HowsYourDayApi.Models;
using HowsYourDayApi.Repositories;

namespace HowsYourDayApi.Services
{
    public class DayEntryService: IDayEntryService
    {
        private readonly IDayEntryRepository _dayRepository;

        public DayEntryService(IDayEntryRepository dayRepository)
        {
            _dayRepository = dayRepository;
        }

        public async Task<IEnumerable<DayEntry>> GetDayEntriesAsync(DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            // Retrieve all days within the specified date range
            var dayEntries = await _dayRepository.SearchAsync(null, fromUtc, toUtc);

            return dayEntries;
        }

        public async Task<DayEntry?> GetDayEntryAsync(Guid dayEntryId)
        {
            var dayEntry = await _dayRepository.GetByIdAsync(dayEntryId);

            return dayEntry;
        }

        public async Task<bool> HasUserPostedTodayAsync(Guid userId)
        {
            var entryToday = (await _dayRepository.SearchAsync(userId, DateTime.UtcNow)).SingleOrDefault();

            return entryToday != null;
        }

        public async Task<IEnumerable<DayEntry>> GetDayEntriesOfUserAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            // Retrieve all days for a specific user within the specified date range
            var dayEntries = await _dayRepository.SearchAsync(userId, fromUtc, toUtc);

            return dayEntries;
        }

        public async Task<DayEntry?> GetDayEntryOfUserTodayAsync(Guid userId)
        {
            var entryToday = (await _dayRepository.SearchAsync(userId, DateTime.UtcNow)).SingleOrDefault();

            return entryToday;
        }

        public async Task InsertDayEntryOfUserAsync(Guid userId, DayEntry day)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (day == null)
                throw new ArgumentNullException(nameof(day), "Day cannot be null.");

            var hasPostedToday = await HasUserPostedTodayAsync(userId);

            if (hasPostedToday)
                throw new InvalidOperationException("User has already posted today.");

            day.Id = Guid.NewGuid();
            day.UserId = userId;
            day.LoggedAtUtc = DateTime.UtcNow;

            await _dayRepository.InsertAsync(day);
        }

        public async Task UpdateDayEntryOfUserAsync(Guid userId, DayEntry entry)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (entry == null)
                throw new ArgumentNullException(nameof(entry), "Day cannot be null.");

            await _dayRepository.UpdateAsync(entry);
        }

        public async Task DeleteDayEntriesOfUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            await _dayRepository.DeleteAllUserEntriesAsync(userId);
        }
    }
}
