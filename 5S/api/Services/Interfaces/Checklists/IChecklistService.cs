using api.DTOS.Checklists;
using api.Helpers.Pagination;

namespace api.Services.Interfaces.Checklists
{
    public interface IChecklistService
    {
        Task<PagedResponse<ChecklistResponseDto>> GetAll(PaginationRequest request);
        Task<List<ChecklistResponseDto>> GetAll();
        Task<List<ChecklistResponseDto>> GetByCategoryId(int categoryId);
        Task<ChecklistResponseDto> GetById(int id);
        Task<int> Create(CreateChecklistDto dto);
        Task Update(UpdateChecklistDto dto);
        Task Delete(int id);
    }
}
