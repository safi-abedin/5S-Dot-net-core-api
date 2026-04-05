using api.DTOS.Checklists;

namespace api.Services.Interfaces.Checklists
{
    public interface IChecklistCategoryService
    {
        Task<List<ChecklistCategoryResponseDto>> GetAll();
    }
}
