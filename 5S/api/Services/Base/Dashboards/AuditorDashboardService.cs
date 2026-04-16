using api.DTOS.Dashboards;
using api.Enums;
using api.Models.Audits;
using api.Models.RedTags;
using api.Models.Users;
using api.Models.Zones;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Dashboards;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.Dashboards
{
    public class AuditorDashboardService : IAuditorDashboardService
    {
        private readonly IRepository<Audit> _auditRepo;
        private readonly IRepository<RedTag> _redTagRepo;
        private readonly IRepository<Zone> _zoneRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuditorDashboardService(
            IRepository<Audit> auditRepo,
            IRepository<RedTag> redTagRepo,
            IRepository<Zone> zoneRepo,
            UserManager<ApplicationUser> userManager)
        {
            _auditRepo = auditRepo;
            _redTagRepo = redTagRepo;
            _zoneRepo = zoneRepo;
            _userManager = userManager;
        }

        public async Task<AuditorDashboardResponseDto> GetByUserId(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("User not found");

            if (!user.CompanyId.HasValue)
                throw new Exception("User is not assigned to a company");

            var companyId = user.CompanyId.Value;

            var auditsQuery = _auditRepo.Query().Where(x => x.CompanyId == companyId);
            var redTagsQuery = _redTagRepo.Query().Where(x => x.CompanyId == companyId);

            var totalAudits = await auditsQuery.CountAsync();
            var completedAudits = await auditsQuery.CountAsync(x => x.Status == AuditStatus.Reviewed);
            var averageAuditPercentage = await auditsQuery.Select(x => (decimal?)x.Percentage).AverageAsync() ?? 0;

            var totalRedTags = await redTagsQuery.CountAsync();
            var closedRedTags = await redTagsQuery.CountAsync(x => x.Status == RedTagStatus.Closed || x.ClosingDate != null);
            var openRedTags = totalRedTags - closedRedTags;

            var avgClosureDays = await redTagsQuery
                .Where(r =>r.ClosingDate != null)
                .Select(r => (double?)EF.Functions.DateDiffDay(r.IdentifiedDate, r.ClosingDate.Value))
                .AverageAsync();

            var auditStatusBreakdown = await auditsQuery
                .GroupBy(x => x.Status)
                .Select(g => new DashboardStatusCountDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var monthlyAuditTrendRaw = await auditsQuery
                .GroupBy(x => new { x.AuditDate.Year, x.AuditDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count(),
                    AveragePercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Take(12)
                .ToListAsync();

            var monthlyAuditTrend = monthlyAuditTrendRaw
                .Select(x => new DashboardTrendPointDto
                {
                    Label = $"{x.Year}-{x.Month:00}",
                    Count = x.Count,
                    AveragePercentage = x.AveragePercentage
                })
                .ToList();

            var zoneAgg = await auditsQuery
                .GroupBy(x => x.ZoneId)
                .Select(g => new
                {
                    ZoneId = g.Key,
                    AuditCount = g.Count(),
                    AveragePercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .ToListAsync();

            var zoneIds = zoneAgg.Select(x => x.ZoneId).Distinct().ToList();
            var zoneNames = await _zoneRepo.Query()
                .Where(x => zoneIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            var zoneMap = zoneNames.ToDictionary(x => x.Id, x => x.Name);

            var zoneInsights = zoneAgg
                .Select(x => new DashboardZoneInsightDto
                {
                    ZoneId = x.ZoneId,
                    ZoneName = zoneMap.TryGetValue(x.ZoneId, out var zoneName) ? zoneName : "Unknown",
                    AuditCount = x.AuditCount,
                    AveragePercentage = x.AveragePercentage
                })
                .OrderBy(x => x.AveragePercentage)
                .ToList();

            var recentAudits = await auditsQuery
                .OrderByDescending(x => x.AuditDate)
                .Take(10)
                .Select(x => new DashboardRecentAuditDto
                {
                    Id = x.Id,
                    ZoneId = x.ZoneId,
                    AuditDate = x.AuditDate,
                    Percentage = x.Percentage,
                    Status = x.Status.ToString()
                })
                .ToListAsync();

            return new AuditorDashboardResponseDto
            {
                UserId = userId,
                CompanyId = companyId,
                UserName = user.Name ?? user.UserName ?? string.Empty,
                TotalAudits = totalAudits,
                CompletedAudits = completedAudits,
                AverageAuditPercentage = averageAuditPercentage,
                TotalRedTags = totalRedTags,
                OpenRedTags = openRedTags,
                ClosedRedTags = closedRedTags,
                AverageRedTagClosureDays = avgClosureDays.HasValue
                                            ? (decimal)avgClosureDays.Value
                                            : 0,
                AuditStatusBreakdown = auditStatusBreakdown,
                MonthlyAuditTrend = monthlyAuditTrend,
                ZoneInsights = zoneInsights,
                RecentAudits = recentAudits
            };
        }
    }
}
