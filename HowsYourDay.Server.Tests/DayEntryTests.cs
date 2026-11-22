using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Tests
{
    public class DayEntryTests
    {
        [Theory(DisplayName = "When rating is between 1 and 10, it is accepted")]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void When_RatingIsBetween1And10_Should_AcceptRating(int validRating)
        {
            var dayEntry = new DayEntry();

            dayEntry.Rating = validRating;

            Assert.Equal(validRating, dayEntry.Rating);
        }

        [Theory(DisplayName = "When rating is outside 1-10, it should throw an exception")]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(11)]
        [InlineData(99)]
        public void When_RatingIsOutside1And10_Should_ThrowException(int invalidRating)
        {
            var dayEntry = new DayEntry();

            Assert.Throws<ArgumentOutOfRangeException>(() => dayEntry.Rating = invalidRating);
        }

        [Theory(DisplayName = "When note is blank, it should throw an exception")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\n\t")]
        public void When_NoteIsBlank_Should_ThrowException(string note)
        {
            var dayEntry = new DayEntry();

            Assert.Throws<ArgumentException>(() => dayEntry.Note = note);
        }

        [Fact(DisplayName = "When note is too long, it should throw an exception")]
        public void When_NoteIsTooLong_Should_ThrowException()
        {
            var dayEntry = new DayEntry();
            var note = new string('a', DayEntry.MaxNoteLength + 1);

            Assert.Throws<ArgumentException>(() => dayEntry.Note = note);
        }

        [Fact(DisplayName = "When note has surrounding whitespace, it should be trimmed")]
        public void When_NoteHasSurroundingWhitespace_Should_Trim()
        {
            var dayEntry = new DayEntry();

            dayEntry.Note = "  A good day  ";

            Assert.Equal("A good day", dayEntry.Note);
        }
    }
}
