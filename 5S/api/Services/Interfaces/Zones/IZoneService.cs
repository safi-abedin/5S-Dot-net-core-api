using api.DTOS.Zones;
using api.Helpers.Pagination;
using api.Models.Zones;

namespace api.Services.Interfaces.Zones
{
    public interface IZoneService
    {
        Task<PagedResponse<ZoneResponseDto>> GetAll(PaginationRequest request);
        Task<List<ZoneResponseDto>> GetAllByCompanyId(int companyId);
        Task<ZoneResponseDto> GetById(int id);
        Task<int> Create(CreateZoneDto dto);
        Task Update(UpdateZoneDto dto);
        Task Delete(int id);
    }
}
