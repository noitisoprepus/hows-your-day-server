using HowsYourDay.Server.Utilities;

namespace HowsYourDay.Server.Tests
{
    public class TimeZoneHelperTests
    {
        [Fact]
        public void GetLocalDate_AcrossMidnight_UsesTimeZone()
        {
            var plus8 = TimeZoneInfo.CreateCustomTimeZone(
                "Test/Plus8",
                TimeSpan.FromHours(8),
                "Plus8",
                "Plus8");
            var utcNow = new DateTime(2026, 11, 22, 23, 30, 0, DateTimeKind.Utc);

            var localDate = TimeZoneHelper.GetLocalDate(utcNow, plus8);

            Assert.Equal(new DateOnly(2026, 11, 23), localDate);
        }

        [Fact]
        public void GetLocalDate_DuringDstTransition_ReturnsCorrectDate()
        {
            var newYork = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            var beforeTransition = new DateTime(2026, 3, 8, 6, 59, 0, DateTimeKind.Utc);
            var afterTransition = new DateTime(2026, 3, 8, 7, 0, 0, DateTimeKind.Utc);

            Assert.Equal(
                new DateOnly(2026, 3, 8),
                TimeZoneHelper.GetLocalDate(beforeTransition, newYork));
            Assert.Equal(
                new DateOnly(2026, 3, 8),
                TimeZoneHelper.GetLocalDate(afterTransition, newYork));
        }

        [Fact]
        public void GetLocalDate_WithUnspecifiedKind_TreatsValueAsUtc()
        {
            var utc = TimeZoneInfo.Utc;
            var utcNow = new DateTime(2026, 11, 22, 23, 30, 0, DateTimeKind.Unspecified);

            var localDate = TimeZoneHelper.GetLocalDate(utcNow, utc);

            Assert.Equal(new DateOnly(2026, 11, 22), localDate);
        }

        [Fact]
        public void Resolve_WithInvalidId_ReturnsFallback()
        {
            var fallback = TimeZoneInfo.Utc;

            var result = TimeZoneHelper.Resolve("Not/A-Real-Time-Zone", fallback);

            Assert.Same(fallback, result);
        }

        [Fact]
        public void Resolve_WithNullId_ReturnsFallback()
        {
            var fallback = TimeZoneInfo.Utc;

            var result = TimeZoneHelper.Resolve(null, fallback);

            Assert.Same(fallback, result);
        }

        [Fact]
        public void Resolve_WithValidId_ReturnsTimeZone()
        {
            var result = TimeZoneHelper.Resolve("Asia/Manila", TimeZoneInfo.Utc);

            Assert.Equal(TimeSpan.FromHours(8), result.BaseUtcOffset);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        [InlineData("Not/A-Real-Time-Zone", false)]
        [InlineData("UTC", true)]
        public void IsValid_ReturnsExpectedResult(string? timeZoneId, bool expected)
        {
            Assert.Equal(expected, TimeZoneHelper.IsValid(timeZoneId));
        }
    }
}
