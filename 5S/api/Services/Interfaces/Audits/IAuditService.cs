using api.DTOS.Audits;
using api.Helpers.Pagination;

namespace api.Services.Interfaces.Audits
{
    public interface IAuditService
    {
        Task<PagedResponse<AuditResponseDto>> GetAll(PaginationRequest request);
        Task<AuditResponseDto> GetById(int id);
        Task<int> Create(CreateAuditDto dto);
        Task Update(UpdateAuditDto dto);
        Task Delete(int id);
    }
}
