using api.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        // 🔐 LOGIN
        public async Task<AuthResponseDto> Login(LoginRequestDto dto)
        {
            var company = _companyRepo.Query()
                .FirstOrDefault(x => x.CompanyCode == dto.CompanyCode && x.IsActive);

            if (company == null)
                throw new Exception("Invalid company");

            var user = await _userManager.FindByNameAsync(dto.Username);

            if (user == null || user.CompanyId != company.Id)
                throw new Exception("Invalid credentials");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!valid)
                throw new Exception("Invalid credentials");

            var token = GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                CompanyId = user.CompanyId,
                Role = user.Role,
                Username = user.UserName
            };
        }

        // 👤 REGISTER (Company Admin and Super Admin creates users)
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


        // 🔑 TOKEN
        private string GenerateToken(ApplicationUser user)
        {
            var claims = new List<Claim>
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("CompanyId", user.CompanyId?.ToString() ?? ""),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(1),
                claims: claims,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
