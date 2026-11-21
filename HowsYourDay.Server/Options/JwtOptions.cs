using System.ComponentModel.DataAnnotations;

namespace HowsYourDay.Server.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    [MinLength(64)]
    public string Key { get; init; } = string.Empty;
}
