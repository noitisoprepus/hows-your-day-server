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

        public async Task<(IReadOnlyList<DayEntry> Entries, bool HasMore)> GetDayEntriesOfUserAsync(
            Guid userId,
            DateOnly? before = null,
            int limit = 20,
            DateOnly? from = null,
            bool analyzedOnly = false)
        {
            EnsureUserId(userId);

            var entries = await _dayRepository.GetPageByUserAsync(userId, before, limit + 1, from, analyzedOnly);
            var hasMore = entries.Count > limit;

            return (entries.Take(limit).ToList(), hasMore);
        }

        public async Task<DayEntry?> GetDayEntryOfUserTodayAsync(Guid userId)
        {
            EnsureUserId(userId);

            var timeZone = await _userTimeZoneService.GetTimeZoneAsync(userId);
            var today = TimeZoneHelper.GetLocalDate(GetUtcNow(), timeZone);

            return await _dayRepository.GetByUserAndDateAsync(userId, today);
        }

        public async Task<DayEntry?> GetDayEntryOfUserByDateAsync(Guid userId, DateOnly entryDate)
        {
            EnsureUserId(userId);

            return await _dayRepository.GetByUserAndDateAsync(userId, entryDate);
        }

        public async Task InsertDayEntryOfUserAsync(Guid userId, DayEntry day, DateOnly? expectedDate = null)
        {
            EnsureUserId(userId);

            if (day == null)
                throw new ArgumentNullException(nameof(day), "Day cannot be null.");

            ValidateEntry(day);

            var timeZone = await _userTimeZoneService.GetTimeZoneAsync(userId);
            var now = GetUtcNow();
            var today = TimeZoneHelper.GetLocalDate(now, timeZone);

            if (expectedDate.HasValue && expectedDate.Value != today)
                throw new DayEntryNotFoundException();

            var existingEntry = await _dayRepository.GetByUserAndDateAsync(userId, today);
            if (existingEntry != null)
                throw new DayEntryAlreadyExistsException();

            day.Id = Guid.NewGuid();
            day.UserId = userId;
            day.EntryDate = today;
            day.LoggedAtUtc = now;

            day.Analysis = _nlpService.Analyze(day);
            await _dayRepository.InsertAsync(day);
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
            if (existingEntry.EntryDate != today || entry.EntryDate != today)
                throw new DayEntryNotFoundException();

            ValidateEntry(entry);

            var analysis = _nlpService.Analyze(entry);
            if (existingEntry.Analysis is { } previous)
            {
                previous.Label = analysis.Label;
                previous.Summary = analysis.Summary;
                previous.SentimentScore = analysis.SentimentScore;
                previous.KeyPhrases = analysis.KeyPhrases;
                previous.Entities = analysis.Entities;
                previous.AnalyzedAtUtc = analysis.AnalyzedAtUtc;
            }
            else
            {
                existingEntry.Analysis = analysis;
            }

            existingEntry.Rating = entry.Rating;
            existingEntry.Note = entry.Note;
            await _dayRepository.UpdateAsync(existingEntry);
            entry.LoggedAtUtc = existingEntry.LoggedAtUtc;
            entry.Analysis = existingEntry.Analysis;
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
