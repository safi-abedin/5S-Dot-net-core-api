using api.Data;
using api.DTOS.Profiles;
using api.Services.Interfaces.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api.Controllers.Profiles
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IFileStorageService _fileStorage;

        public ProfileController(AppDbContext db, IFileStorageService fileStorage)
        {
            _db = db;
            _fileStorage = fileStorage;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetLoggedInUserId();

            var profile = await _db.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.UserName,
                    u.Role,
                    u.CompanyId
                })
                .FirstOrDefaultAsync();

            if (profile == null)
                throw new Exception("User not found");

            var company = profile.CompanyId.HasValue
                ? await _db.Companies.AsNoTracking()
                    .Where(c => c.Id == profile.CompanyId.Value && !c.IsDeleted)
                    .Select(c => new
                    {
                        c.CompanyName,
                        c.CompanyAddress,
                        c.LogoUrl,
                        c.Email,
                        c.Phone
                    })
                    .FirstOrDefaultAsync()
                : null;

            return Ok(new UserProfileResponseDto
            {
                UserId = profile.Id,
                Name = profile.Name ?? string.Empty,
                UserName = profile.UserName ?? string.Empty,
                Role = profile.Role ?? string.Empty,
                CompanyId = profile.CompanyId,
                CompanyName = company?.CompanyName ?? string.Empty,
                CompanyAddress = company?.CompanyAddress ?? string.Empty,
                CompanyLogoUrl = company?.LogoUrl ?? string.Empty,
                Email = company?.Email ?? string.Empty,
                Phone = company?.Phone ?? string.Empty
            });
        }

        [HttpPost("company-branding")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCompanyBranding([FromForm] UpdateCompanyBrandingDto dto)
        {
            var userId = GetLoggedInUserId();

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("User not found");

            if (!user.CompanyId.HasValue)
                throw new Exception("User is not mapped to a company");

            var company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == user.CompanyId.Value && !x.IsDeleted);
            if (company == null)
                throw new Exception("Company not found");

            if (!string.IsNullOrWhiteSpace(dto.CompanyAddress))
                company.CompanyAddress = dto.CompanyAddress;

            if (dto.Logo != null && dto.Logo.Length > 0)
            {
                company.LogoUrl = await _fileStorage.SaveAsync(dto.Logo, "company-logos");
            }

            company.LastUpdatedAt = DateTime.UtcNow;

            _db.Companies.Update(company);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                company.Id,
                company.CompanyAddress,
                company.LogoUrl
            });
        }

        private int GetLoggedInUserId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("UserId");

            if (!int.TryParse(claimValue, out var userId))
                throw new UnauthorizedAccessException("Invalid user claim");

            return userId;
        }
    }
}
