using api.DTOS.Dashboards;

namespace api.Services.Interfaces.Dashboards
{
    public interface IAuditorDashboardService
    {
        Task<AuditorDashboardResponseDto> GetByUserId(int userId);
    }
}
