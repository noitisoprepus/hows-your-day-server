namespace HowsYourDayApi.Models;

public class DayEntryAnalysis
{
    public Guid Id { get; set; }
    public Guid DayEntryId { get; set; }
    public Guid UserId { get; set; }
    
    // NLP results
    public List<string> KeyPhrases { get; set; }
    public List<string> Entities { get; set; } // Named entities
    
    public DateTime AnalyzedAtUtc { get; set; }
}