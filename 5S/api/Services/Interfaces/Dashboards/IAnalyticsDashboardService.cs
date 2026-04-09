using api.DTOS.Dashboards;

namespace api.Services.Interfaces.Dashboards
{
    public interface IAnalyticsDashboardService
    {
        Task<AnalyticsBasicDashboardDto> GetBasic(int? companyId, int days);
        Task<AnalyticsAdvancedDashboardDto> GetAdvanced(int? companyId, int days);
    }
}
