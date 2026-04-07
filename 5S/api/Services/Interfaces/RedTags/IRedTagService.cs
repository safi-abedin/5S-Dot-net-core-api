using api.DTOS.RedTags;
using api.Helpers.Pagination;

namespace api.Services.Interfaces.RedTags
{
    public interface IRedTagService
    {
        Task<PagedResponse<RedTagResponseDto>> GetAll(PaginationRequest request);
        Task<List<RedTagResponseDto>> GetAllByCompanyId(int companyId);
        Task<RedTagResponseDto> GetById(int id);
        Task<int> Create(CreateRedTagDto dto);
        Task Update(UpdateRedTagDto dto);
        Task Delete(int id);
    }
}
