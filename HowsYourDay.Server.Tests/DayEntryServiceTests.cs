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
        public async Task HasUserPostedTodayAsync_ReturnsTrue_WhenEntryExistsToday()
        {
            var userId = Guid.NewGuid();
            var repository = new FakeDayEntryRepository(
                CreateEntry(userId, Now.UtcDateTime.AddHours(-1)));
            var service = CreateService(repository);

            var result = await service.HasUserPostedTodayAsync(userId);

            Assert.True(result);
        }

        [Fact]
        public async Task HasUserPostedTodayAsync_ReturnsFalse_WhenOnlyYesterdayHasEntry()
        {
            var userId = Guid.NewGuid();
            var repository = new FakeDayEntryRepository(
                CreateEntry(userId, Now.UtcDateTime.AddDays(-1)));
            var service = CreateService(repository);

            var result = await service.HasUserPostedTodayAsync(userId);

            Assert.False(result);
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
            var analysisRepository = new FakeDayEntryAnalysisRepository();
            var service = CreateService(repository, analysisRepository);
            var entry = new DayEntry
            {
                Rating = 5,
                Note = "A good day"
            };

            await service.InsertDayEntryOfUserAsync(userId, entry);

            Assert.Equal(userId, entry.UserId);
            Assert.Equal(Now.UtcDateTime, entry.LoggedAtUtc);
            Assert.Single(repository.Entries);
            Assert.Single(analysisRepository.Analyses);
            Assert.Equal(entry.Id, analysisRepository.Analyses[0].DayEntryId);
        }

        private static DayEntry CreateEntry(Guid userId, DateTime loggedAtUtc)
        {
            return new DayEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LoggedAtUtc = loggedAtUtc,
                Rating = 7,
                Note = "A good day"
            };
        }

        private static DayEntryService CreateService(
            FakeDayEntryRepository repository,
            FakeDayEntryAnalysisRepository? analysisRepository = null)
        {
            return new DayEntryService(
                repository,
                analysisRepository ?? new FakeDayEntryAnalysisRepository(),
                new FakeNlpService(),
                new FixedTimeProvider(Now));
        }
    }

    internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    internal sealed class FakeDayEntryRepository(params DayEntry[] entries) : IDayEntryRepository
    {
        public List<DayEntry> Entries { get; } = [.. entries];

        public Task<DayEntry?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Entries.FirstOrDefault(entry => entry.Id == id));
        }

        public Task<IEnumerable<DayEntry>> SearchAsync(
            Guid? userId = null,
            DateTime? fromUtc = null,
            DateTime? toUtc = null)
        {
            var query = Entries.AsEnumerable();

            if (userId.HasValue)
                query = query.Where(entry => entry.UserId == userId.Value);
            if (fromUtc.HasValue)
                query = query.Where(entry => entry.LoggedAtUtc >= fromUtc.Value);
            if (toUtc.HasValue)
                query = query.Where(entry => entry.LoggedAtUtc <= toUtc.Value);

            return Task.FromResult(query.ToList().AsEnumerable());
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
                KeyPhrases = [],
                Entities = [],
                AnalyzedAtUtc = DateTime.UtcNow
            };
        }
    }
}
