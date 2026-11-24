using System.Text.Json;
using System.Text.RegularExpressions;
using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Services;

public static class SentimentAnalyzer
{
    private static readonly Regex WordPattern = new("[a-z]+(?:'[a-z]+)?", RegexOptions.Compiled);

    private static readonly SentimentLexicon Lexicon = LoadLexicon();

    private static SentimentLexicon LoadLexicon()
    {
        using var resource = typeof(SentimentAnalyzer).Assembly.GetManifestResourceStream("SentimentLexicon.json")
            ?? throw new InvalidOperationException("The sentiment lexicon resource is missing.");

        return JsonSerializer.Deserialize<SentimentLexicon>(resource)
            ?? throw new InvalidOperationException("The sentiment lexicon is empty.");
    }

    private sealed class SentimentLexicon
    {
        public required HashSet<string> PositiveWords { get; init; }
        public required HashSet<string> NegativeWords { get; init; }
        public required HashSet<string> Negators { get; init; }
    }

    public static SentimentResult Analyze(string text)
    {
        var positive = 0;
        var negative = 0;
        var negateWindow = 0;

        foreach (var word in Tokenize(text))
        {
            if (Lexicon.Negators.Contains(word))
            {
                negateWindow = 3;
                continue;
            }

            if (Lexicon.PositiveWords.Contains(word))
            {
                if (negateWindow > 0)
                {
                    negative++;
                    negateWindow = 0;
                }
                else
                {
                    positive++;
                }
            }
            else if (Lexicon.NegativeWords.Contains(word))
            {
                if (negateWindow > 0)
                {
                    positive++;
                    negateWindow = 0;
                }
                else
                {
                    negative++;
                }
            }
            else if (negateWindow > 0)
            {
                negateWindow--;
            }
        }

        var total = positive + negative;
        var score = total == 0 ? 0 : (double)(positive - negative) / total;

        return new SentimentResult(GetLabel(score, positive, negative), Math.Round(score, 2));
    }

    public static string BuildSummary(string label, IReadOnlyList<string> keyPhrases)
    {
        var tone = label switch
        {
            DayEntryAnalysis.PositiveLabel => "The entry reads as positive overall.",
            DayEntryAnalysis.MostlyPositiveLabel => "The entry reads as mostly positive.",
            DayEntryAnalysis.MixedLabel => "The entry mixes positive and negative feelings.",
            DayEntryAnalysis.MostlyNegativeLabel => "The entry reads as mostly negative.",
            DayEntryAnalysis.NegativeLabel => "The entry reads as negative overall.",
            _ => "The entry reads as neutral overall."
        };

        if (keyPhrases.Count == 0)
            return tone;

        var themes = string.Join(", ", keyPhrases.Take(3));
        var summary = $"{tone} Recurring themes: {themes}.";

        if (summary.Length <= DayEntryAnalysis.MaxSummaryLength)
            return summary;

        return summary[..(DayEntryAnalysis.MaxSummaryLength - 1)].TrimEnd(' ', ',') + "…";
    }

    private static string GetLabel(double score, int positive, int negative)
    {
        if (score >= 0.5)
            return DayEntryAnalysis.PositiveLabel;
        if (score > 0)
            return DayEntryAnalysis.MostlyPositiveLabel;
        if (score == 0)
            return positive > 0 && negative > 0
                ? DayEntryAnalysis.MixedLabel
                : DayEntryAnalysis.NeutralLabel;
        if (score > -0.5)
            return DayEntryAnalysis.MostlyNegativeLabel;

        return DayEntryAnalysis.NegativeLabel;
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        var normalized = text
            .Replace('’', '\'')
            .Replace('‘', '\'')
            .ToLowerInvariant();

        return WordPattern
            .Matches(normalized)
            .Select(match => match.Value);
    }
}

public record SentimentResult(string Label, double Score);
