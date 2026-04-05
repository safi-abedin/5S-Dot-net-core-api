using api.DTOS.Users;
using api.Helpers.Pagination;

namespace api.Services.Interfaces.Users
{
    public interface IUserManagementService
    {
        Task<PagedResponse<UserResponseDto>> GetAll(PaginationRequest request);
        Task<UserResponseDto> GetById(int id);
        Task<int> Create(CreateUserDto dto);
        Task Update(UpdateUserDto dto);
        Task Delete(int id);
    }
}
