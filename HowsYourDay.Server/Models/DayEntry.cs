namespace HowsYourDay.Server.Models
{
    public class DayEntry
    {
        public const int MinRating = 1;
        public const int MaxRating = 10;
        public const int MaxNoteLength = 4000;

        private int _rating;
        private string _note = string.Empty;

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime LoggedAtUtc { get; set; }

        public int Rating
        {
            get => _rating;
            set
            {
                if (value is < MinRating or > MaxRating)
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Rating must be between {MinRating} and {MaxRating}.");

                _rating = value;
            }
        }

        public string Note
        {
            get => _note;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Note is required.", nameof(value));

                var trimmed = value.Trim();
                if (trimmed.Length > MaxNoteLength)
                    throw new ArgumentException(
                        $"Note cannot exceed {MaxNoteLength} characters.",
                        nameof(value));

                _note = trimmed;
            }
        }
    }
}
