using api.DTOS.Auths;

namespace api.Services.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Login(LoginRequestDto dto);
        Task<AuthResponseDto> RefreshToken(RefreshTokenRequestDto dto);
        Task<int> RegisterUser(RegisterUserDto dto);
    }
}
