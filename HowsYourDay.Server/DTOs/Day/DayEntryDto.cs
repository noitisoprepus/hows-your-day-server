namespace HowsYourDay.Server.DTOs.Day
{
    public class DayEntryDto
    {
        public Guid Id { get; set; }
        public DateTime LoggedAtUtc { get; set; }
        public int Rating { get; set; }
        public string Note { get; set; } = string.Empty;
    }   
}
