using HowsYourDay.Server.Models;
using HowsYourDay.Server.Services;

namespace HowsYourDay.Server.Tests
{
    public class SentimentAnalyzerTests
    {
        [Fact]
        public void Analyze_WithPositiveText_ReturnsPositiveLabel()
        {
            var result = SentimentAnalyzer.Analyze("I had a great day, felt happy and proud.");

            Assert.Equal(DayEntryAnalysis.PositiveLabel, result.Label);
            Assert.True(result.Score > 0);
        }

        [Fact]
        public void Analyze_WithNegativeText_ReturnsNegativeLabel()
        {
            var result = SentimentAnalyzer.Analyze("I felt awful, sad and exhausted.");

            Assert.Equal(DayEntryAnalysis.NegativeLabel, result.Label);
            Assert.True(result.Score < 0);
        }

        [Fact]
        public void Analyze_WithNeutralText_ReturnsNeutralLabel()
        {
            var result = SentimentAnalyzer.Analyze("I went to the store and bought milk.");

            Assert.Equal(DayEntryAnalysis.NeutralLabel, result.Label);
            Assert.Equal(0, result.Score);
        }

        [Fact]
        public void Analyze_WithEmptyText_ReturnsNeutralLabel()
        {
            var result = SentimentAnalyzer.Analyze("");

            Assert.Equal(DayEntryAnalysis.NeutralLabel, result.Label);
            Assert.Equal(0, result.Score);
        }

        [Fact]
        public void Analyze_WithBalancedText_ReturnsMixedLabel()
        {
            var result = SentimentAnalyzer.Analyze("I was happy but also sad.");

            Assert.Equal(DayEntryAnalysis.MixedLabel, result.Label);
            Assert.Equal(0, result.Score);
        }

        [Fact]
        public void Analyze_WithNegatedPositive_ReturnsNegativeScore()
        {
            var result = SentimentAnalyzer.Analyze("I was not happy.");

            Assert.True(result.Score < 0);
        }

        [Fact]
        public void Analyze_WithNegatedNegative_ReturnsPositiveScore()
        {
            var result = SentimentAnalyzer.Analyze("I was not sad.");

            Assert.True(result.Score > 0);
        }

        [Fact]
        public void Analyze_WithCurlyApostropheNegation_ReturnsNegativeScore()
        {
            var result = SentimentAnalyzer.Analyze("I wasn’t happy.");

            Assert.True(result.Score < 0);
        }

        [Theory]
        [InlineData("I worked hard today and shipped the feature.")]
        [InlineData("I walked down to the cafe.")]
        [InlineData("I spent the afternoon alone.")]
        [InlineData("I broke my old record.")]
        [InlineData("It was dark outside when I got home.")]
        public void Analyze_WithAmbiguousWords_ReturnsNeutralLabel(string text)
        {
            var result = SentimentAnalyzer.Analyze(text);

            Assert.Equal(DayEntryAnalysis.NeutralLabel, result.Label);
            Assert.Equal(0, result.Score);
        }

        [Theory]
        [InlineData("happy glad proud sad", DayEntryAnalysis.PositiveLabel)]
        [InlineData("happy glad sad", DayEntryAnalysis.MostlyPositiveLabel)]
        [InlineData("happy sad tired", DayEntryAnalysis.MostlyNegativeLabel)]
        [InlineData("happy sad tired awful", DayEntryAnalysis.NegativeLabel)]
        public void Analyze_WithDifferentScoreRatios_ReturnsExpectedLabel(
            string text,
            string expectedLabel)
        {
            var result = SentimentAnalyzer.Analyze(text);

            Assert.Equal(expectedLabel, result.Label);
        }

        [Fact]
        public void BuildSummary_IncludesToneAndThemes()
        {
            var summary = SentimentAnalyzer.BuildSummary(
                DayEntryAnalysis.MostlyPositiveLabel,
                ["stressful meeting", "team"]);

            Assert.StartsWith("The entry reads as mostly positive.", summary);
            Assert.Contains("stressful meeting", summary);
        }

        [Fact]
        public void BuildSummary_WithoutThemes_ReturnsToneOnly()
        {
            var summary = SentimentAnalyzer.BuildSummary(DayEntryAnalysis.NeutralLabel, []);

            Assert.Equal("The entry reads as neutral overall.", summary);
        }

        [Fact]
        public void BuildSummary_WithLongThemes_StaysWithinLimit()
        {
            var summary = SentimentAnalyzer.BuildSummary(
                DayEntryAnalysis.NegativeLabel,
                [new string('a', 300)]);

            Assert.True(summary.Length <= DayEntryAnalysis.MaxSummaryLength);
            Assert.EndsWith("…", summary);
        }
    }
}
