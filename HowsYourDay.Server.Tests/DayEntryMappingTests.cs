using HowsYourDay.Server.DTOs.Day;
using HowsYourDay.Server.Mappings;
using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Tests
{
    public class DayEntryMappingTests
    {
        [Fact]
        public void ToDto_MapsAllFields()
        {
            var entry = new DayEntry
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EntryDate = new DateOnly(2026, 11, 22),
                LoggedAtUtc = new DateTime(2026, 11, 22, 8, 30, 0, DateTimeKind.Utc),
                Rating = 7,
                Note = "A good day"
            };

            var dto = entry.ToDto();

            Assert.Equal(entry.Id, dto.Id);
            Assert.Equal(entry.EntryDate, dto.EntryDate);
            Assert.Equal(entry.LoggedAtUtc, dto.LoggedAtUtc);
            Assert.Equal(entry.Rating, dto.Rating);
            Assert.Equal(entry.Note, dto.Note);
        }

        [Fact]
        public void ToEntity_MapsRatingAndNote()
        {
            var dto = new CreateDayEntryDto
            {
                Rating = 4,
                Note = "Busy but productive"
            };

            var entry = dto.ToEntity();

            Assert.Equal(dto.Rating, entry.Rating);
            Assert.Equal(dto.Note, entry.Note);
            Assert.Equal(Guid.Empty, entry.Id);
            Assert.Equal(Guid.Empty, entry.UserId);
            Assert.Equal(DateOnly.MinValue, entry.EntryDate);
        }

        [Fact]
        public void ApplyTo_UpdatesRatingAndNote()
        {
            var entry = new DayEntry
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EntryDate = new DateOnly(2026, 11, 22),
                LoggedAtUtc = new DateTime(2026, 11, 22, 8, 30, 0, DateTimeKind.Utc),
                Rating = 2,
                Note = "Old note"
            };
            var dto = new CreateDayEntryDto
            {
                Rating = 9,
                Note = "New note"
            };

            dto.ApplyTo(entry);

            Assert.Equal(dto.Rating, entry.Rating);
            Assert.Equal(dto.Note, entry.Note);
            Assert.Equal(new DateOnly(2026, 11, 22), entry.EntryDate);
            Assert.NotEqual(Guid.Empty, entry.Id);
        }
    }
}
