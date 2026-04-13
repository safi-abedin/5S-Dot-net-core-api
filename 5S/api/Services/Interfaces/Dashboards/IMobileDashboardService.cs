using api.DTOS.Dashboards;

namespace api.Services.Interfaces.Dashboards
{
    public interface IMobileDashboardService
    {
        Task<MobileDashboardSummaryDto> GetSummary(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<MobileDashboardTrendDto>> GetTrend(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<MobileDashboardCategoryScoreDto>> GetCategoryScores(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<MobileDashboardRecentAuditDto>> GetRecentAudits(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null);
        Task<MobileDashboardPerformanceDto> GetPerformance(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<MobileDashboardRecentTagDto>> GetRecentTags(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
