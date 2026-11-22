namespace HowsYourDay.Server.Utilities;

public static class TimeZoneHelper
{
    public static bool TryFind(string? timeZoneId, out TimeZoneInfo timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }

        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            timeZone = TimeZoneInfo.Utc;
            return false;
        }
    }

    public static TimeZoneInfo Resolve(string? timeZoneId, TimeZoneInfo fallback)
    {
        return TryFind(timeZoneId, out var timeZone) ? timeZone : fallback;
    }

    public static bool IsValid(string? timeZoneId)
    {
        return TryFind(timeZoneId, out _);
    }

    public static DateOnly GetLocalDate(DateTime utcNow, TimeZoneInfo timeZone)
    {
        var utc = utcNow.Kind switch
        {
            DateTimeKind.Utc => utcNow,
            DateTimeKind.Local => utcNow.ToUniversalTime(),
            _ => DateTime.SpecifyKind(utcNow, DateTimeKind.Utc)
        };

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone));
    }
}
