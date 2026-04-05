using api.DTOS.Users;
using api.Helpers.Pagination;
using api.Models.Users;
using api.Services.Interfaces.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.Users
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser,
            ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<PagedResponse<UserResponseDto>> GetAll(PaginationRequest request)
        {
            var query = GetScopedUsers()
                .OrderByDescending(x => x.Id)
                .Select(x => new UserResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Username = x.UserName,
                    Role = x.Role,
                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.CompanyName
                });

            return await PaginationHelper.CreateAsync(query, request.Page, request.Size);
        }

        public async Task<UserResponseDto> GetById(int id)
        {
            var user = await GetScopedUsers()
                .Where(x => x.Id == id)
                .Select(x => new UserResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Username = x.UserName,
                    Role = x.Role,
                    CompanyId = x.CompanyId,
                    CompanyName = x.Company.CompanyName
                })
                .FirstOrDefaultAsync();

            if (user == null)
                throw new Exception("User not found");

            return user;
        }

        public async Task<int> Create(CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                throw new Exception("Username is required");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new Exception("Password is required");

            var existing = await _userManager.FindByNameAsync(dto.Username);
            if (existing != null)
                throw new Exception("Username already exists");

            var companyId = dto.CompanyId;

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Name = dto.Name,
                Role = dto.Role,
                CompanyId = companyId
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                throw new Exception(string.Join(",", result.Errors.Select(x => x.Description)));

            _logger.LogInformation("User created: {Username}", dto.Username);

            return user.Id;
        }

        public async Task Update(UpdateUserDto dto)
        {
            var user = await GetScopedUsers().FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (user == null)
                throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(dto.Username) && !string.Equals(user.UserName, dto.Username, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.FindByNameAsync(dto.Username);
                if (existing != null && existing.Id != dto.Id)
                    throw new Exception("Username already exists");

                user.UserName = dto.Username;
            }

            user.Name = dto.Name;
            user.Role = dto.Role;

            if (_currentUser.CompanyId == 0 && dto.CompanyId.HasValue)
                user.CompanyId = dto.CompanyId;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new Exception(string.Join(",", updateResult.Errors.Select(x => x.Description)));

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!passwordResult.Succeeded)
                    throw new Exception(string.Join(",", passwordResult.Errors.Select(x => x.Description)));
            }

            _logger.LogInformation("User updated: {Id}", dto.Id);
        }

        public async Task Delete(int id)
        {
            var user = await GetScopedUsers().FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                throw new Exception("User not found");

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception(string.Join(",", result.Errors.Select(x => x.Description)));

            _logger.LogWarning("User deleted: {Id}", id);
        }

        private IQueryable<ApplicationUser> GetScopedUsers()
        {
            var companyId = _currentUser.CompanyId;
            var query = _userManager.Users;

            if (companyId > 1)
                query = query.Where(x => x.CompanyId == companyId);

            return query;
        }

    }
}
