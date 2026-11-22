using System.ComponentModel.DataAnnotations;
using HowsYourDay.Server.DTOs.Day;
using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Tests
{
    public class CreateDayEntryDtoTests
    {
        [Theory(DisplayName = "Rating outside 1-10 is invalid")]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(11)]
        [InlineData(99)]
        public void Rating_OutsideRange_IsInvalid(int rating)
        {
            var dto = new CreateDayEntryDto
            {
                Rating = rating,
                Note = "A good day"
            };

            var isValid = TryValidate(dto, out var results);

            Assert.False(isValid);
            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(CreateDayEntryDto.Rating)));
        }

        [Theory(DisplayName = "Rating inside 1-10 is valid")]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void Rating_InsideRange_IsValid(int rating)
        {
            var dto = new CreateDayEntryDto
            {
                Rating = rating,
                Note = "A good day"
            };

            Assert.True(TryValidate(dto, out _));
        }

        [Theory(DisplayName = "Blank note is invalid")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\n\t")]
        public void Note_Blank_IsInvalid(string note)
        {
            var dto = new CreateDayEntryDto
            {
                Rating = 5,
                Note = note
            };

            var isValid = TryValidate(dto, out var results);

            Assert.False(isValid);
            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(CreateDayEntryDto.Note)));
        }

        [Fact(DisplayName = "Null note is invalid")]
        public void Note_Null_IsInvalid()
        {
            var dto = new CreateDayEntryDto
            {
                Rating = 5,
                Note = null!
            };

            Assert.False(TryValidate(dto, out _));
        }

        [Fact(DisplayName = "Note longer than the maximum is invalid")]
        public void Note_TooLong_IsInvalid()
        {
            var dto = new CreateDayEntryDto
            {
                Rating = 5,
                Note = new string('a', DayEntry.MaxNoteLength + 1)
            };

            var isValid = TryValidate(dto, out var results);

            Assert.False(isValid);
            Assert.Contains(results, result =>
                result.MemberNames.Contains(nameof(CreateDayEntryDto.Note)));
        }

        [Fact(DisplayName = "Note at the maximum length is valid")]
        public void Note_AtMaxLength_IsValid()
        {
            var dto = new CreateDayEntryDto
            {
                Rating = 5,
                Note = new string('a', DayEntry.MaxNoteLength)
            };

            Assert.True(TryValidate(dto, out _));
        }

        private static bool TryValidate(object model, out List<ValidationResult> results)
        {
            results = [];
            return Validator.TryValidateObject(
                model,
                new ValidationContext(model),
                results,
                validateAllProperties: true);
        }
    }
}
