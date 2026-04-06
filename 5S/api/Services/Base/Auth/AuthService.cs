using api.DTOS.Auths;
using api.Models.Companies;
using api.Models.Users;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Auth;
using api.Services.Interfaces.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace api.Services.Base.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Company> _companyRepo;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IRepository<Company> companyRepo,
            IConfiguration config,
            ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _companyRepo = companyRepo;
            _config = config;
            _currentUser = currentUser;
        }

        public async Task<AuthResponseDto> Login(LoginRequestDto dto)
        {
            var company = _companyRepo.Query()
                .FirstOrDefault(x => x.CompanyCode == dto.CompanyCode && x.IsActive);

            if (company == null)
                throw new UnauthorizedAccessException("Invalid company");

            var user = await _userManager.FindByNameAsync(dto.Username);

            if (user == null || user.CompanyId != company.Id)
                throw new UnauthorizedAccessException("Invalid credentials");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
                throw new UnauthorizedAccessException("Invalid credentials");

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var token = GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = user.RefreshToken,
                CompanyId = user.CompanyId,
                Role = user.Role,
                Username = user.UserName,
                UserId = user.Id
            };
        }

        public async Task<AuthResponseDto> RefreshToken(RefreshTokenRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.RefreshToken))
                throw new UnauthorizedAccessException("Invalid token request");

            var principal = GetPrincipalFromExpiredToken(dto.Token);
            var userId = principal.FindFirst("UserId")?.Value;

            if (!int.TryParse(userId, out var id))
                throw new UnauthorizedAccessException("Invalid token payload");

            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                throw new UnauthorizedAccessException("Invalid token user");

            if (user.RefreshToken != dto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid refresh token");

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var newAccessToken = GenerateToken(user);

            return new AuthResponseDto
            {
                Token = newAccessToken,
                RefreshToken = user.RefreshToken,
                CompanyId = user.CompanyId,
                Role = user.Role,
                Username = user.UserName,
                UserId = user.Id
            };
        }

        public async Task<int> RegisterUser(RegisterUserDto dto)
        {
            var companyId = _currentUser.CompanyId;

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Name = dto.Name,
                CompanyId = companyId,
                Role = dto.Role
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(",", result.Errors.Select(x => x.Description)));

            return user.Id;
        }

        private string GenerateToken(ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(_config["Jwt:Key"]))
                throw new Exception("JWT key is missing");

            var role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role;

            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("CompanyId", user.CompanyId?.ToString() ?? "0"),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(1),
                claims: claims,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var key = _config["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("JWT key is missing");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            return principal;
        }
    }
}
