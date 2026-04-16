namespace HowsYourDay.Server.DTOs.Authentication
{
    public class UserDto
    {
        public required Guid Id { get; set; }
        public required string Username { get; set; }
    }   
}