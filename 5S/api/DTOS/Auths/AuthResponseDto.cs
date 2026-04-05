namespace api.DTOS.Auths
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public int? CompanyId { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
    }
}
