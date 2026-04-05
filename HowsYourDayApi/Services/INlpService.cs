using HowsYourDayApi.Models;

namespace HowsYourDayApi.Services;

public interface INlpService
{
    Task<DayEntryAnalysis> AnalyzeAsync(DayEntry entry);
}