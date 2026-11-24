using System.ComponentModel.DataAnnotations;

namespace HowsYourDay.Server.Options;

public class AppOptions
{
    public const string SectionName = "App";

    [Required]
    public string DefaultTimeZone { get; init; } = "UTC";

    public bool HttpsRedirectionEnabled { get; init; } = true;

    public string? TrustedProxy { get; init; }

    public string? DataProtectionKeyPath { get; init; }
}
