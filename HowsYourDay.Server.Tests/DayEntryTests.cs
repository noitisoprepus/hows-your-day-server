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

        [Theory(DisplayName = "When rating is outside 1�10, it should throw an exception")]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(11)]
        [InlineData(99)]
        public void When_RatingIsOutside1And10_Should_ThrowException(int invalidRating)
        {
            var dayEntry = new DayEntry();

            Assert.Throws<ArgumentOutOfRangeException>(() => dayEntry.Rating = invalidRating);
        }
    }

}