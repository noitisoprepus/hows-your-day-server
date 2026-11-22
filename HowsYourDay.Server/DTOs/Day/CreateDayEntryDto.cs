using System.ComponentModel.DataAnnotations;
using HowsYourDay.Server.Models;

namespace HowsYourDay.Server.DTOs.Day
{
    public class CreateDayEntryDto : IValidatableObject
    {
        [Range(DayEntry.MinRating, DayEntry.MaxRating)]
        public int Rating { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(DayEntry.MaxNoteLength, MinimumLength = 1)]
        public string Note { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Note is not null && Note.Length > 0 && string.IsNullOrWhiteSpace(Note))
                yield return new ValidationResult("The Note field is required.", [nameof(Note)]);
        }
    }
}
