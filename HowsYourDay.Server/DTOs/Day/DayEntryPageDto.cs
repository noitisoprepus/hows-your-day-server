namespace HowsYourDay.Server.DTOs.Day
{
    public class DayEntryPageDto
    {
        public IReadOnlyList<DayEntryDto> Entries { get; set; } = [];
        public bool HasMore { get; set; }
    }
}
