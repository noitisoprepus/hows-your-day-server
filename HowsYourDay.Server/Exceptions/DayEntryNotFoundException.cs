namespace HowsYourDay.Server.Exceptions;

public class DayEntryNotFoundException : Exception
{
    public DayEntryNotFoundException()
        : base("The day entry was not found.")
    {
    }
}
