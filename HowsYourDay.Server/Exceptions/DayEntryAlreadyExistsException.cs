namespace HowsYourDay.Server.Exceptions;

public class DayEntryAlreadyExistsException : Exception
{
    public DayEntryAlreadyExistsException()
        : base("An entry already exists for today.")
    {
    }
}
