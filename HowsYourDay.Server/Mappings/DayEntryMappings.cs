using HowsYourDay.Server.DTOs.Day;
using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.Mappings;

public static class DayEntryMappings
{
    public static DayEntryDto ToDto(this DayEntry entry)
    {
        return new DayEntryDto
        {
            Id = entry.Id,
            EntryDate = entry.EntryDate,
            LoggedAtUtc = entry.LoggedAtUtc,
            Rating = entry.Rating,
            Note = entry.Note
        };
    }

    public static DayEntry ToEntity(this CreateDayEntryDto dto)
    {
        return new DayEntry
        {
            Rating = dto.Rating,
            Note = dto.Note
        };
    }

    public static void ApplyTo(this CreateDayEntryDto dto, DayEntry entry)
    {
        entry.Rating = dto.Rating;
        entry.Note = dto.Note;
    }
}
