using HowsYourDay.Server.Models;
using HowsYourDay.Server.Options;
using HowsYourDay.Server.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HowsYourDay.Server.Services;

public class UserTimeZoneService : IUserTimeZoneService
{
    private readonly UserManager<AppUser> _userManager;

    public UserTimeZoneService(UserManager<AppUser> userManager, IOptions<AppOptions> appOptions)
    {
        _userManager = userManager;
        DefaultTimeZone = TimeZoneHelper.Resolve(appOptions.Value.DefaultTimeZone, TimeZoneInfo.Utc);
    }

    public TimeZoneInfo DefaultTimeZone { get; }

    public async Task<TimeZoneInfo> GetTimeZoneAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return TimeZoneHelper.Resolve(user?.TimeZoneId, DefaultTimeZone);
    }
}
