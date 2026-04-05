namespace api.DTOS.Users
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public int? CompanyId { get; set; }

        public string? CompanyName { get; set; }
    }
}
