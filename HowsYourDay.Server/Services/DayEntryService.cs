using HowsYourDay.Server.Exceptions;
using HowsYourDay.Server.Models;
using HowsYourDay.Server.Repositories;

namespace HowsYourDay.Server.Services
{
    public class DayEntryService : IDayEntryService
    {
        private readonly IDayEntryRepository _dayRepository;
        private readonly IDayEntryAnalysisRepository _dayEntryAnalysisRepository;
        private readonly INlpService _nlpService;
        private readonly TimeProvider _timeProvider;

        public DayEntryService(
            IDayEntryRepository dayRepository,
            IDayEntryAnalysisRepository dayEntryAnalysisRepository,
            INlpService nlpService,
            TimeProvider timeProvider)
        {
            _dayRepository = dayRepository;
            _dayEntryAnalysisRepository = dayEntryAnalysisRepository;
            _nlpService = nlpService;
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
            ValidateUserId(userId);

            var entries = await GetEntriesOfUserForUtcDayAsync(userId, GetUtcNow());

            return entries.Count > 0;
        }

        public async Task<IEnumerable<DayEntry>> GetDayEntriesOfUserAsync(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null)
        {
            ValidateUserId(userId);

            // Retrieve all days for a specific user within the specified date range
            var dayEntries = await _dayRepository.SearchAsync(userId, fromUtc, toUtc);

            return dayEntries;
        }

        public async Task<DayEntry?> GetDayEntryOfUserTodayAsync(Guid userId)
        {
            ValidateUserId(userId);

            var entries = await GetEntriesOfUserForUtcDayAsync(userId, GetUtcNow());

            return entries
                .OrderByDescending(entry => entry.LoggedAtUtc)
                .FirstOrDefault();
        }

        public async Task InsertDayEntryOfUserAsync(Guid userId, DayEntry day)
        {
            ValidateUserId(userId);

            if (day == null)
                throw new ArgumentNullException(nameof(day), "Day cannot be null.");

            ValidateEntry(day);

            var now = GetUtcNow();
            var entriesToday = await GetEntriesOfUserForUtcDayAsync(userId, now);

            if (entriesToday.Count > 0)
                throw new DayEntryAlreadyExistsException();

            day.Id = Guid.NewGuid();
            day.UserId = userId;
            day.LoggedAtUtc = now;

            await _dayRepository.InsertAsync(day);

            var analysis = _nlpService.Analyze(day);
            await _dayEntryAnalysisRepository.InsertAsync(analysis);
        }

        public async Task UpdateDayEntryOfUserAsync(Guid userId, DayEntry entry)
        {
            ValidateUserId(userId);

            if (entry == null)
                throw new ArgumentNullException(nameof(entry), "Day cannot be null.");

            var existingEntry = await _dayRepository.GetByIdAsync(entry.Id);
            if (existingEntry == null || existingEntry.UserId != userId)
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
            ValidateUserId(userId);

            await _dayRepository.DeleteAllUserEntriesAsync(userId);
            await _dayEntryAnalysisRepository.DeleteAllUserEntriesAsync(userId);
        }

        private DateTime GetUtcNow()
        {
            return _timeProvider.GetUtcNow().UtcDateTime;
        }

        private async Task<IReadOnlyList<DayEntry>> GetEntriesOfUserForUtcDayAsync(Guid userId, DateTime utcNow)
        {
            var startUtc = utcNow.Date;
            var endUtc = startUtc.AddDays(1).AddTicks(-1);

            var entries = await _dayRepository.SearchAsync(userId, startUtc, endUtc);

            return entries.ToList();
        }

        private static void ValidateUserId(Guid userId)
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
