using System.Text.RegularExpressions;
using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Services;

public static class SentimentAnalyzer
{
    private static readonly Regex WordPattern = new("[a-z]+(?:'[a-z]+)?", RegexOptions.Compiled);

    private static readonly HashSet<string> PositiveWords = new(StringComparer.Ordinal)
    {
        "good", "great", "happy", "glad", "joy", "joyful", "excited", "exciting", "proud",
        "grateful", "thankful", "love", "loved", "loving", "like", "liked", "enjoy",
        "enjoyed", "enjoying", "fun", "wonderful", "amazing", "awesome", "fantastic",
        "excellent", "pleasant", "nice", "lovely", "beautiful", "calm", "relaxed",
        "peaceful", "hopeful", "optimistic", "confident", "motivated", "productive",
        "accomplished", "success", "successful", "win", "won", "better", "best",
        "improve", "improved", "improvement", "laugh", "laughed", "laughing", "smile",
        "smiled", "smiling", "cheerful", "delighted", "content", "satisfied",
        "satisfying", "energized", "energetic", "fresh", "rested", "healthy", "safe",
        "supported", "appreciated", "kind", "friendly", "warm", "bright", "comfortable",
        "relief", "relieved", "lucky", "blessed", "thrilled", "ecstatic"
    };

    private static readonly HashSet<string> NegativeWords = new(StringComparer.Ordinal)
    {
        "bad", "worse", "worst", "sad", "unhappy", "miserable", "depressed", "depressing",
        "awful", "terrible", "horrible", "hate", "hated", "dislike", "angry",
        "anger", "mad", "annoyed", "annoying", "frustrated", "frustrating", "frustration",
        "stressed", "stressful", "stress", "anxious", "anxiety", "worried", "worry",
        "nervous", "scared", "afraid", "fear", "fearful", "lonely", "empty",
        "tired", "exhausted", "drained", "burnout", "sick", "ill", "pain", "hurt",
        "hurting", "crying", "cried", "cry", "tears", "upset", "disappointed",
        "disappointing", "disappointment", "hopeless", "helpless", "worthless", "guilty",
        "ashamed", "embarrassed", "regret", "regretful", "grief", "grieving", "loss",
        "fail", "failed", "failure", "problem", "problems", "conflict", "fight",
        "fought", "argument", "argue", "trouble", "difficult", "struggling",
        "struggle", "overwhelmed", "overwhelming", "pressure", "bored", "boring", "dull",
        "gloomy", "negative", "pessimistic", "panic", "rage", "furious",
        "irritated", "sleepless", "insomnia", "headache", "broken", "unsafe",
        "threatened"
    };

    private static readonly HashSet<string> Negators = new(StringComparer.Ordinal)
    {
        "not", "no", "never", "nothing", "nobody", "none", "neither", "nor", "cannot",
        "can't", "couldn't", "didn't", "doesn't", "don't", "isn't", "wasn't", "weren't",
        "won't", "wouldn't", "shouldn't", "haven't", "hasn't", "hadn't", "hardly",
        "barely", "without"
    };

    public static SentimentResult Analyze(string text)
    {
        var positive = 0;
        var negative = 0;
        var negateWindow = 0;

        foreach (var word in Tokenize(text))
        {
            if (Negators.Contains(word))
            {
                negateWindow = 3;
                continue;
            }

            if (PositiveWords.Contains(word))
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
            else if (NegativeWords.Contains(word))
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
