using api.DTOS.Auths;
using api.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        // 🔐 LOGIN (PUBLIC)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var result = await _service.Login(dto);
            return Ok(result);
        }

        // 🔄 REFRESH TOKEN
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
        {
            var result = await _service.RefreshToken(dto);
            return Ok(result);
        }

        // 👤 CREATE USER (Company Admin)
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            var id = await _service.RegisterUser(dto);
            return Ok(new { Id = id });
        }
    }
}
