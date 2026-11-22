namespace HowsYourDay.Server.Services;

public interface IUserTimeZoneService
{
    TimeZoneInfo DefaultTimeZone { get; }

    Task<TimeZoneInfo> GetTimeZoneAsync(Guid userId);
}
