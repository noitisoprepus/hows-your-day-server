namespace HowsYourDay.Server.Models;

public class DayEntryAnalysis
{
    public const int MaxLabelLength = 32;
    public const int MaxSummaryLength = 280;

    public const string PositiveLabel = "Positive";
    public const string MostlyPositiveLabel = "Mostly positive";
    public const string NeutralLabel = "Neutral";
    public const string MixedLabel = "Mixed";
    public const string MostlyNegativeLabel = "Mostly negative";
    public const string NegativeLabel = "Negative";

    public Guid Id { get; set; }
    public Guid DayEntryId { get; set; }
    public Guid UserId { get; set; }
    
    // NLP results
    public string Label { get; set; } = NeutralLabel;
    public string Summary { get; set; } = string.Empty;
    public double SentimentScore { get; set; }
    public List<string> KeyPhrases { get; set; } = [];
    public List<string> Entities { get; set; } = []; // Named entities
    
    public DateTime AnalyzedAtUtc { get; set; }
}
