using HowsYourDayApi.Models;

namespace HowsYourDayApi.Services;

public interface INlpService
{
    DayEntryAnalysis Analyze(DayEntry entry);
}