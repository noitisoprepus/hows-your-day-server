using Catalyst;
using Catalyst.Models;
using HowsYourDay.Server.Models;
using Mosaik.Core;
using Version = Mosaik.Core.Version;

namespace HowsYourDay.Server.Services;

public class NlpService : INlpService
{
    private Pipeline? _nlp;

    public NlpService()
    {
        InitializeAsync().GetAwaiter().GetResult();
    }
    
    private async Task InitializeAsync()
    {
        if (_nlp != null) return;

        Catalyst.Models.English.Register();

        _nlp = await Pipeline.ForAsync(Language.English);
        _nlp.Add(await AveragePerceptronEntityRecognizer.FromStoreAsync(Language.English, Version.Latest, "WikiNER"));
    }

    public DayEntryAnalysis Analyze(DayEntry entry)
    {
        var doc = new Document(entry.Note, Language.English);
        _nlp.ProcessSingle(doc);

        var entities = ExtractEntities(doc);
        var keyPhrases = ExtractKeyPhrases(doc);

        var analysis = new DayEntryAnalysis
        {
            DayEntryId = entry.Id,
            UserId = entry.UserId,
            Entities = entities,
            KeyPhrases = keyPhrases,
            AnalyzedAtUtc = DateTime.UtcNow
        };

        return analysis;
    }
    
    private List<string> ExtractKeyPhrases(IDocument doc)
    {
        var phrases = new List<string>();

        foreach (var sentence in doc)
        {
            var tokens = sentence.ToList();

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                // Adjective + Noun (e.g., "stressful meeting")
                if (i < tokens.Count - 1 &&
                    token.POS == PartOfSpeech.ADJ &&
                    tokens[i + 1].POS == PartOfSpeech.NOUN)
                {
                    phrases.Add($"{token.Value} {tokens[i + 1].Value}");
                }

                // Verb + Noun (e.g., "finished project")
                if (i < tokens.Count - 1 &&
                    token.POS == PartOfSpeech.VERB &&
                    tokens[i + 1].POS == PartOfSpeech.NOUN)
                {
                    phrases.Add($"{token.Value} {tokens[i + 1].Value}");
                }

                // Emotion pattern (e.g., "felt proud")
                if (token.Value.ToLower() == "felt" && i < tokens.Count - 1)
                {
                    phrases.Add($"{token.Value} {tokens[i + 1].Value}");
                }

                // Standalone important nouns
                if (token.POS == PartOfSpeech.NOUN)
                {
                    phrases.Add(token.Value);
                }
            }
        }

        return phrases
            .Select(p => p.ToLowerInvariant())
            .Where(p => p.Length > 3)
            .GroupBy(p => p)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(10) // limit noise
            .ToList();
    }
    
    private List<string> ExtractEntities(IDocument doc)
    {
        return doc
            .SelectMany(span => span.GetEntities())
            .Select(e => e.Value)
            .Distinct()
            .ToList();
    }
}