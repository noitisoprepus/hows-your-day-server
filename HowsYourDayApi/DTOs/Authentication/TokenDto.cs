namespace HowsYourDayApi.DTOs.Authentication
{
    public record TokenDto(string AccessToken, string RefreshToken);
}