using HowsYourDayApi.DTOs.Day;
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

        public async Task<IEnumerable<DayEntryDto>> GetDayEntriesAsync()
        {
            // Retrieve all day entries
            var dayEntries = await _dayRepository.GetAllAsync();

            // Map the day entries to DayEntryDto
            var dayEntriesDto = dayEntries.Select(entry => new DayEntryDto
            {
                LogDate = entry.LogDateUtc.ToLocalTime(),
                Rating = entry.Rating,
                Note = entry.Note
            });

            return dayEntriesDto;
        }

        public async Task<DayEntryDto?> GetDayEntryAsync(Guid dayEntryId)
        {
            var dayEntry = await _dayRepository.GetByIdAsync(dayEntryId);

            var entryDto = new DayEntryDto();

            if (dayEntry != null)
            {
                // Map the entry to DayEntryDto
                entryDto.LogDate = dayEntry.LogDateUtc.ToLocalTime();
                entryDto.Rating = dayEntry.Rating;
                entryDto.Note = dayEntry.Note;
            }

            return entryDto;
        }

        public async Task<bool> HasUserPostedTodayAsync(Guid userId)
        {
            var entryToday = (await _dayRepository.SearchAsync(userId, DateTime.UtcNow)).SingleOrDefault();

            return entryToday != null;
        }

        public async Task<IEnumerable<DayEntryDto>> GetDayEntriesOfUserAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            // Retrieve all days for a specific user within the specified date range
            var dayEntries = await _dayRepository.SearchAsync(userId, fromUtc, toUtc);
            
            // Map the day entries to DayEntryDto
            var dayEntriesDto = dayEntries.Select(entry => new DayEntryDto
            {
                LogDate = entry.LogDateUtc.ToLocalTime(),
                Rating = entry.Rating,
                Note = entry.Note
            });

            return dayEntriesDto;
        }

        public async Task<DayEntryDto> GetDayEntryOfUserTodayAsync(Guid userId)
        {
            var entryToday = (await _dayRepository.SearchAsync(userId, DateTime.UtcNow)).SingleOrDefault();

            var entryTodayDto = new DayEntryDto();

            if (entryToday != null)
            {
                // Map the entry to DayEntryDto
                entryTodayDto.LogDate = entryToday.LogDateUtc.ToLocalTime();
                entryTodayDto.Rating = entryToday.Rating;
                entryTodayDto.Note = entryToday.Note;
            }

            return entryTodayDto;
        }

        public async Task InsertDayEntryOfUserAsync(Guid userId, CreateDayEntryDto day)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (day == null)
                throw new ArgumentNullException(nameof(day), "Day cannot be null.");

            var hasPostedToday = await HasUserPostedTodayAsync(userId);

            if (hasPostedToday)
                throw new InvalidOperationException("User has already posted today.");

            var newDayEntry = new DayEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LogDateUtc = DateTime.UtcNow,
                Rating = day.Rating,
                Note = day.Note
            };

            await _dayRepository.InsertAsync(newDayEntry);
        }

        public async Task UpdateDayEntryOfUserTodayAsync(Guid userId, CreateDayEntryDto day)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (day == null)
                throw new ArgumentNullException(nameof(day), "Day cannot be null.");

            var entryToday = (await _dayRepository.SearchAsync(userId, DateTime.UtcNow)).SingleOrDefault();

            if (entryToday == null)
                throw new InvalidOperationException("User has not posted today.");

            entryToday.Rating = day.Rating;
            entryToday.Note = day.Note;

            await _dayRepository.UpdateAsync(entryToday);
        }
    }
}
