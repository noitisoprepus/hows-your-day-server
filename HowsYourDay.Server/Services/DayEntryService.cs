using HowsYourDay.Server.Exceptions;
using HowsYourDay.Server.Models;
using HowsYourDay.Server.Repositories;
using HowsYourDay.Server.Utilities;

namespace HowsYourDay.Server.Services
{
    public class DayEntryService : IDayEntryService
    {
        private readonly IDayEntryRepository _dayRepository;
        private readonly IDayEntryAnalysisRepository _dayEntryAnalysisRepository;
        private readonly INlpService _nlpService;
        private readonly IUserTimeZoneService _userTimeZoneService;
        private readonly TimeProvider _timeProvider;

        public DayEntryService(
            IDayEntryRepository dayRepository,
            IDayEntryAnalysisRepository dayEntryAnalysisRepository,
            INlpService nlpService,
            IUserTimeZoneService userTimeZoneService,
            TimeProvider timeProvider)
        {
            _dayRepository = dayRepository;
            _dayEntryAnalysisRepository = dayEntryAnalysisRepository;
            _nlpService = nlpService;
            _userTimeZoneService = userTimeZoneService;
            _timeProvider = timeProvider;
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
            EnsureUserId(userId);

            return await GetDayEntryOfUserTodayAsync(userId) is not null;
        }

        public async Task<IEnumerable<DayEntry>> GetDayEntriesOfUserAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            EnsureUserId(userId);

            // Retrieve all days for a specific user within the specified date range
            var dayEntries = await _dayRepository.SearchAsync(userId, fromUtc, toUtc);

            return dayEntries;
        }

        public async Task<DayEntry?> GetDayEntryOfUserTodayAsync(Guid userId)
        {
            EnsureUserId(userId);

            var timeZone = await _userTimeZoneService.GetTimeZoneAsync(userId);
            var today = TimeZoneHelper.GetLocalDate(GetUtcNow(), timeZone);

            return await _dayRepository.GetByUserAndDateAsync(userId, today);
        }

        public async Task InsertDayEntryOfUserAsync(Guid userId, DayEntry day)
        {
            EnsureUserId(userId);

            if (day == null)
                throw new ArgumentNullException(nameof(day), "Day cannot be null.");

            ValidateEntry(day);

            var timeZone = await _userTimeZoneService.GetTimeZoneAsync(userId);
            var now = GetUtcNow();
            var today = TimeZoneHelper.GetLocalDate(now, timeZone);

            var existingEntry = await _dayRepository.GetByUserAndDateAsync(userId, today);
            if (existingEntry != null)
                throw new DayEntryAlreadyExistsException();

            day.Id = Guid.NewGuid();
            day.UserId = userId;
            day.EntryDate = today;
            day.LoggedAtUtc = now;

            await _dayRepository.InsertAsync(day);

            var analysis = _nlpService.Analyze(day);
            await _dayEntryAnalysisRepository.InsertAsync(analysis);
        }

        public async Task UpdateDayEntryOfUserAsync(Guid userId, DayEntry entry)
        {
            EnsureUserId(userId);

            if (entry == null)
                throw new ArgumentNullException(nameof(entry), "Day cannot be null.");

            var existingEntry = await _dayRepository.GetByIdAsync(entry.Id);
            if (existingEntry == null || existingEntry.UserId != userId)
                throw new DayEntryNotFoundException();

            var timeZone = await _userTimeZoneService.GetTimeZoneAsync(userId);
            var today = TimeZoneHelper.GetLocalDate(GetUtcNow(), timeZone);
            if (entry.EntryDate != today)
                throw new DayEntryNotFoundException();

            ValidateEntry(entry);

            await _dayRepository.UpdateAsync(entry);

            var existingAnalysis = await _dayEntryAnalysisRepository.GetByDayEntryIdAsync(entry.Id);
            if (existingAnalysis != null)
            {
                var newAnalysis = _nlpService.Analyze(entry);
                existingAnalysis.KeyPhrases = newAnalysis.KeyPhrases;
                existingAnalysis.Entities = newAnalysis.Entities;
                existingAnalysis.AnalyzedAtUtc = GetUtcNow();
                await _dayEntryAnalysisRepository.UpdateAsync(existingAnalysis);
            }
            else
            {
                var newAnalysis = _nlpService.Analyze(entry);
                await _dayEntryAnalysisRepository.InsertAsync(newAnalysis);
            }
        }

        public async Task DeleteDayEntriesOfUserAsync(Guid userId)
        {
            EnsureUserId(userId);

            await _dayRepository.DeleteAllUserEntriesAsync(userId);
            await _dayEntryAnalysisRepository.DeleteAllUserEntriesAsync(userId);
        }

        private DateTime GetUtcNow()
        {
            return _timeProvider.GetUtcNow().UtcDateTime;
        }

        private static void EnsureUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        }

        private static void ValidateEntry(DayEntry day)
        {
            if (day.Rating is < DayEntry.MinRating or > DayEntry.MaxRating)
                throw new ArgumentOutOfRangeException(
                    nameof(day.Rating),
                    day.Rating,
                    $"Rating must be between {DayEntry.MinRating} and {DayEntry.MaxRating}.");

            if (string.IsNullOrWhiteSpace(day.Note))
                throw new ArgumentException("Note is required.", nameof(day.Note));

            if (day.Note.Length > DayEntry.MaxNoteLength)
                throw new ArgumentException(
                    $"Note cannot exceed {DayEntry.MaxNoteLength} characters.",
                    nameof(day.Note));
        }
    }
}
