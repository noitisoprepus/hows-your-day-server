namespace HowsYourDay.Server.Models
{
    public class DayEntry
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime LoggedAtUtc { get; set; }
        public int Rating { get; set; }
        public string Note { get; set; }
    }
}