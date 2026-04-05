using api.DTOS.Companies;
using api.Helpers.Pagination;

namespace api.Services.Interfaces.Companies
{
    public interface ICompanyService
    {
        Task<PagedResponse<CompanyResponseDto>> GetAll(PaginationRequest request);
        Task<CompanyResponseDto> GetById(int id);
        Task<int> Create(CreateCompanyDto dto);
        Task Update(UpdateCompanyDto dto);
        Task Delete(int id);
    }
}
