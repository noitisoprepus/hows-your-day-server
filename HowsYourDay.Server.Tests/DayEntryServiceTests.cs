using HowsYourDay.Server.Exceptions;
using HowsYourDay.Server.Models;
using HowsYourDay.Server.Repositories;
using HowsYourDay.Server.Services;

namespace HowsYourDay.Server.Tests
{
    public class DayEntryServiceTests
    {
        private static readonly DateTimeOffset Now = new(2026, 11, 22, 12, 0, 0, TimeSpan.Zero);

        [Fact]
        public async Task GetDayEntryOfUserTodayAsync_ReturnsEntryLoggedToday()
        {
            var userId = Guid.NewGuid();
            var entry = CreateEntry(userId, Now.UtcDateTime.AddHours(-1));
            var repository = new FakeDayEntryRepository(entry);
            var service = CreateService(repository);

            var result = await service.GetDayEntryOfUserTodayAsync(userId);

            Assert.Same(entry, result);
        }

        [Fact]
        public async Task GetDayEntryOfUserTodayAsync_DoesNotReturnEntryFromYesterday()
        {
            var userId = Guid.NewGuid();
            var entry = CreateEntry(userId, Now.UtcDateTime.AddDays(-1));
            var repository = new FakeDayEntryRepository(entry);
            var service = CreateService(repository);

            var result = await service.GetDayEntryOfUserTodayAsync(userId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDayEntryOfUserTodayAsync_UsesUserTimeZone()
        {
            var userId = Guid.NewGuid();
            var plus8 = TimeZoneInfo.CreateCustomTimeZone(
                "Test/Plus8",
                TimeSpan.FromHours(8),
                "Plus8",
                "Plus8");
            var now = new DateTimeOffset(2026, 11, 22, 23, 30, 0, TimeSpan.Zero);
            var todayEntry = CreateEntry(userId, now.UtcDateTime, new DateOnly(2026, 11, 23));
            var previousEntry = CreateEntry(
                userId,
                now.UtcDateTime.AddDays(-1),
                new DateOnly(2026, 11, 22));
            var repository = new FakeDayEntryRepository(todayEntry, previousEntry);
            var service = CreateService(repository, timeZone: plus8, now: now);

            var result = await service.GetDayEntryOfUserTodayAsync(userId);

            Assert.Same(todayEntry, result);
        }

        [Fact]
        public async Task GetDayEntriesOfUserAsync_ReturnsNewestFirstAndPaginates()
        {
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var newest = CreateEntry(userId, Now.UtcDateTime, new DateOnly(2026, 11, 22));
            var middle = CreateEntry(userId, Now.UtcDateTime.AddDays(-1), new DateOnly(2026, 11, 21));
            var oldest = CreateEntry(userId, Now.UtcDateTime.AddDays(-2), new DateOnly(2026, 11, 20));
            var otherUser = CreateEntry(otherUserId, Now.UtcDateTime, new DateOnly(2026, 11, 22));
            var repository = new FakeDayEntryRepository(oldest, otherUser, newest, middle);
            var service = CreateService(repository);

            var firstPage = await service.GetDayEntriesOfUserAsync(userId, limit: 2);

            Assert.Equal(new[] { newest, middle }, firstPage.Entries);
            Assert.True(firstPage.HasMore);

            var secondPage = await service.GetDayEntriesOfUserAsync(userId, middle.EntryDate, 2);

            Assert.Equal(new[] { oldest }, secondPage.Entries);
            Assert.False(secondPage.HasMore);
        }

        [Fact]
        public async Task GetDayEntryOfUserByDateAsync_ReturnsOnlyOwnEntry()
        {
            var userId = Guid.NewGuid();
            var entryDate = new DateOnly(2026, 11, 22);
            var entry = CreateEntry(userId, Now.UtcDateTime, entryDate);
            var repository = new FakeDayEntryRepository(entry);
            var service = CreateService(repository);

            Assert.Same(entry, await service.GetDayEntryOfUserByDateAsync(userId, entryDate));
            Assert.Null(await service.GetDayEntryOfUserByDateAsync(Guid.NewGuid(), entryDate));
        }

        [Fact]
        public async Task InsertDayEntryOfUserAsync_Throws_WhenEntryExistsToday()
        {
            var userId = Guid.NewGuid();
            var repository = new FakeDayEntryRepository(
                CreateEntry(userId, Now.UtcDateTime.AddHours(-1)));
            var service = CreateService(repository);
            var entry = new DayEntry
            {
                Rating = 5,
                Note = "Another day"
            };

            await Assert.ThrowsAsync<DayEntryAlreadyExistsException>(
                () => service.InsertDayEntryOfUserAsync(userId, entry));
        }

        [Fact]
        public async Task InsertDayEntryOfUserAsync_SetsOwnerTimestampAndAnalysis()
        {
            var userId = Guid.NewGuid();
            var repository = new FakeDayEntryRepository();
            var service = CreateService(repository);
            var entry = new DayEntry
            {
                Rating = 5,
                Note = "A good day"
            };

            await service.InsertDayEntryOfUserAsync(userId, entry);

            Assert.Equal(userId, entry.UserId);
            Assert.Equal(DateOnly.FromDateTime(Now.UtcDateTime), entry.EntryDate);
            Assert.Equal(Now.UtcDateTime, entry.LoggedAtUtc);
            Assert.Single(repository.Entries);
            Assert.NotNull(repository.Entries[0].Analysis);
            Assert.Equal(entry.Id, repository.Entries[0].Analysis!.DayEntryId);
        }

        [Fact]
        public async Task InsertDayEntryOfUserAsync_UsesUserTimeZoneForEntryDate()
        {
            var userId = Guid.NewGuid();
            var plus8 = TimeZoneInfo.CreateCustomTimeZone(
                "Test/Plus8",
                TimeSpan.FromHours(8),
                "Plus8",
                "Plus8");
            var now = new DateTimeOffset(2026, 11, 22, 23, 30, 0, TimeSpan.Zero);
            var repository = new FakeDayEntryRepository();
            var service = CreateService(repository, timeZone: plus8, now: now);
            var entry = new DayEntry
            {
                Rating = 5,
                Note = "A good day"
            };

            await service.InsertDayEntryOfUserAsync(userId, entry);

            Assert.Equal(new DateOnly(2026, 11, 23), entry.EntryDate);
        }

        [Fact]
        public async Task UpdateDayEntryOfUserAsync_Throws_WhenEntryIsNotFromToday()
        {
            var userId = Guid.NewGuid();
            var previousEntry = CreateEntry(
                userId,
                Now.UtcDateTime.AddDays(-1),
                DateOnly.FromDateTime(Now.UtcDateTime).AddDays(-1));
            var repository = new FakeDayEntryRepository(previousEntry);
            var service = CreateService(repository);

            await Assert.ThrowsAsync<DayEntryNotFoundException>(
                () => service.UpdateDayEntryOfUserAsync(userId, previousEntry));
        }

        [Fact]
        public async Task UpdateDayEntryOfUserAsync_RefreshesAnalysisInsight()
        {
            var userId = Guid.NewGuid();
            var entry = CreateEntry(userId, Now.UtcDateTime.AddHours(-1));
            var repository = new FakeDayEntryRepository(entry);
            entry.Analysis = new DayEntryAnalysis
            {
                Id = Guid.NewGuid(),
                DayEntryId = entry.Id,
                UserId = userId,
                Label = DayEntryAnalysis.PositiveLabel,
                Summary = "Old summary.",
                SentimentScore = 0.9
            };
            var service = CreateService(repository);

            entry.Note = "An updated note";
            await service.UpdateDayEntryOfUserAsync(userId, entry);

            var analysis = Assert.IsType<DayEntryAnalysis>(entry.Analysis);
            Assert.Equal(DayEntryAnalysis.NeutralLabel, analysis.Label);
            Assert.Equal("The entry reads as neutral overall.", analysis.Summary);
            Assert.Equal(0, analysis.SentimentScore);
            Assert.Same(analysis, entry.Analysis);
        }

        [Fact]
        public async Task InsertDayEntryOfUserAsync_RejectsDraftForPreviousLocalDate()
        {
            var userId = Guid.NewGuid();
            var repository = new FakeDayEntryRepository();
            var service = CreateService(repository);
            var draft = new DayEntry { Rating = 6, Note = "A late draft" };

            await Assert.ThrowsAsync<DayEntryNotFoundException>(() =>
                service.InsertDayEntryOfUserAsync(userId, draft, new DateOnly(2026, 11, 21)));

            Assert.Empty(repository.Entries);
        }

        [Fact]
        public async Task GetDayEntriesOfUserAsync_AppliesBoundsAndInsightFilterBeforePaging()
        {
            var userId = Guid.NewGuid();
            var before = new DateOnly(2026, 12, 1);
            var from = new DateOnly(2026, 11, 1);
            DayEntry Entry(DateOnly date, bool analyzed)
            {
                var entry = CreateEntry(userId, Now.UtcDateTime, date);
                if (analyzed)
                    entry.Analysis = new DayEntryAnalysis();
                return entry;
            }

            var expected = Entry(from, true);
            var repository = new FakeDayEntryRepository(
                Entry(before, true), Entry(from.AddDays(1), false), expected, Entry(from.AddDays(-1), true));

            var page = await CreateService(repository)
                .GetDayEntriesOfUserAsync(userId, before, limit: 1, from: from, analyzedOnly: true);

            Assert.Same(expected, Assert.Single(page.Entries));
            Assert.False(page.HasMore);
        }

        [Fact]
        public async Task InsertDayEntryOfUserAsync_DoesNotPersistEntryWhenAnalysisFails()
        {
            var repository = new FakeDayEntryRepository();
            var service = new DayEntryService(repository, new FakeDayEntryAnalysisRepository(),
                new FailingNlpService(), new FakeUserTimeZoneService(TimeZoneInfo.Utc), new FixedTimeProvider(Now));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.InsertDayEntryOfUserAsync(Guid.NewGuid(), new DayEntry { Rating = 6, Note = "A draft" }));

            Assert.Empty(repository.Entries);
        }

        private sealed class FailingNlpService : INlpService
        {
            public DayEntryAnalysis Analyze(DayEntry entry) => throw new InvalidOperationException();
        }

        private static DayEntry CreateEntry(
            Guid userId,
            DateTime loggedAtUtc,
            DateOnly? entryDate = null)
        {
            return new DayEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EntryDate = entryDate ?? DateOnly.FromDateTime(loggedAtUtc),
                LoggedAtUtc = loggedAtUtc,
                Rating = 7,
                Note = "A good day"
            };
        }

        private static DayEntryService CreateService(
            FakeDayEntryRepository repository,
            FakeDayEntryAnalysisRepository? analysisRepository = null,
            TimeZoneInfo? timeZone = null,
            DateTimeOffset? now = null)
        {
            return new DayEntryService(
                repository,
                analysisRepository ?? new FakeDayEntryAnalysisRepository(),
                new FakeNlpService(),
                new FakeUserTimeZoneService(timeZone ?? TimeZoneInfo.Utc),
                new FixedTimeProvider(now ?? Now));
        }
    }

    internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    internal sealed class FakeUserTimeZoneService(TimeZoneInfo timeZone) : IUserTimeZoneService
    {
        public TimeZoneInfo DefaultTimeZone => timeZone;

        public Task<TimeZoneInfo> GetTimeZoneAsync(Guid userId)
        {
            return Task.FromResult(timeZone);
        }
    }

    internal sealed class FakeDayEntryRepository(params DayEntry[] entries) : IDayEntryRepository
    {
        public List<DayEntry> Entries { get; } = [.. entries];

        public Task<DayEntry?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Entries.FirstOrDefault(entry => entry.Id == id));
        }

        public Task<DayEntry?> GetByUserAndDateAsync(Guid userId, DateOnly entryDate)
        {
            return Task.FromResult(
                Entries.FirstOrDefault(entry =>
                    entry.UserId == userId && entry.EntryDate == entryDate));
        }

        public Task<IReadOnlyList<DayEntry>> GetPageByUserAsync(Guid userId, DateOnly? before, int take, DateOnly? from = null, bool analyzedOnly = false)
        {
            var page = Entries
                .Where(entry => entry.UserId == userId)
                .Where(entry => !before.HasValue || entry.EntryDate < before.Value)
                .Where(entry => !from.HasValue || entry.EntryDate >= from.Value)
                .Where(entry => !analyzedOnly || entry.Analysis is not null)
                .OrderByDescending(entry => entry.EntryDate)
                .Take(take)
                .ToList();

            return Task.FromResult<IReadOnlyList<DayEntry>>(page);
        }

        public Task InsertAsync(DayEntry day)
        {
            Entries.Add(day);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(DayEntry day)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(DayEntry day)
        {
            Entries.Remove(day);
            return Task.CompletedTask;
        }

        public Task DeleteAllUserEntriesAsync(Guid userId)
        {
            Entries.RemoveAll(entry => entry.UserId == userId);
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeDayEntryAnalysisRepository : IDayEntryAnalysisRepository
    {
        public List<DayEntryAnalysis> Analyses { get; } = [];

        public Task<DayEntryAnalysis?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Analyses.FirstOrDefault(analysis => analysis.Id == id));
        }

        public Task<DayEntryAnalysis?> GetByDayEntryIdAsync(Guid dayEntryId)
        {
            return Task.FromResult(
                Analyses.FirstOrDefault(analysis => analysis.DayEntryId == dayEntryId));
        }

        public Task<IEnumerable<DayEntryAnalysis>> SearchAsync(
            Guid? userId = null,
            DateTime? fromUtc = null,
            DateTime? toUtc = null)
        {
            var query = Analyses.AsEnumerable();

            if (userId.HasValue)
                query = query.Where(analysis => analysis.UserId == userId.Value);
            if (fromUtc.HasValue)
                query = query.Where(analysis => analysis.AnalyzedAtUtc >= fromUtc.Value);
            if (toUtc.HasValue)
                query = query.Where(analysis => analysis.AnalyzedAtUtc <= toUtc.Value);

            return Task.FromResult(query.ToList().AsEnumerable());
        }

        public Task InsertAsync(DayEntryAnalysis analysis)
        {
            Analyses.Add(analysis);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(DayEntryAnalysis analysis)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(DayEntryAnalysis analysis)
        {
            Analyses.Remove(analysis);
            return Task.CompletedTask;
        }

        public Task DeleteAllUserEntriesAsync(Guid userId)
        {
            Analyses.RemoveAll(analysis => analysis.UserId == userId);
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeNlpService : INlpService
    {
        public DayEntryAnalysis Analyze(DayEntry entry)
        {
            return new DayEntryAnalysis
            {
                Id = Guid.NewGuid(),
                DayEntryId = entry.Id,
                UserId = entry.UserId,
                Label = DayEntryAnalysis.NeutralLabel,
                Summary = "The entry reads as neutral overall.",
                SentimentScore = 0,
                KeyPhrases = [],
                Entities = [],
                AnalyzedAtUtc = DateTime.UtcNow
            };
        }
    }
}
