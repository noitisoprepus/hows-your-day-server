using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Services;

public interface INlpService
{
    DayEntryAnalysis Analyze(DayEntry entry);
}