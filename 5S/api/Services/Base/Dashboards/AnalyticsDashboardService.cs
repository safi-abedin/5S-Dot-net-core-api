using api.DTOS.Dashboards;
using api.Enums;
using api.Models.Audits;
using api.Models.RedTags;
using api.Models.Zones;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Dashboards;
using api.Services.Interfaces.Users;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.Dashboards
{
    public class AnalyticsDashboardService : IAnalyticsDashboardService
    {
        private readonly IRepository<Audit> _auditRepo;
        private readonly IRepository<RedTag> _redTagRepo;
        private readonly IRepository<Zone> _zoneRepo;
        private readonly IRepository<FeedBackItem> _feedBackRepo;
        private readonly ICurrentUserService _currentUser;

        public AnalyticsDashboardService(
            IRepository<Audit> auditRepo,
            IRepository<RedTag> redTagRepo,
            IRepository<Zone> zoneRepo,
            IRepository<FeedBackItem> feedBackRepo,
            ICurrentUserService currentUser)
        {
            _auditRepo = auditRepo;
            _redTagRepo = redTagRepo;
            _zoneRepo = zoneRepo;
            _feedBackRepo = feedBackRepo;
            _currentUser = currentUser;
        }

        public async Task<AnalyticsBasicDashboardDto> GetBasic(int? companyId, int days)
        {
            var effectiveDays = days <= 0 ? 30 : days;
            var fromDate = DateTime.UtcNow.AddDays(-effectiveDays);

            var auditsQuery = BuildAuditScopedQuery(companyId).Where(x => x.AuditDate >= fromDate);
            var redTagsQuery = BuildRedTagScopedQuery(companyId).Where(x => x.IdentifiedDate >= fromDate);

            var totalAudits = await auditsQuery.CountAsync();
            var averageAuditPercentage = await auditsQuery.Select(x => (decimal?)x.Percentage).AverageAsync() ?? 0;
            var totalRedTags = await redTagsQuery.CountAsync();
            var closedRedTags = await redTagsQuery.CountAsync(x => x.Status == RedTagStatus.Closed || x.ClosingDate != null);
            var openRedTags = totalRedTags - closedRedTags;

            var auditStatusBreakdown = await auditsQuery
                .GroupBy(x => x.Status)
                .Select(g => new DashboardStatusCountDto { Status = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var redTagStatusBreakdown = await redTagsQuery
                .GroupBy(x => x.Status)
                .Select(g => new DashboardStatusCountDto { Status = g.Key.ToString(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var dailyAuditTrendRaw = await auditsQuery
                .GroupBy(x => x.AuditDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    AveragePercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var dailyRedTagTrendRaw = await redTagsQuery
                .GroupBy(x => x.IdentifiedDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return new AnalyticsBasicDashboardDto
            {
                CompanyId = ResolveCompanyIdForResponse(companyId),
                PeriodDays = effectiveDays,
                TotalAudits = totalAudits,
                AverageAuditPercentage = averageAuditPercentage,
                TotalRedTags = totalRedTags,
                OpenRedTags = openRedTags,
                ClosedRedTags = closedRedTags,
                AuditStatusBreakdown = auditStatusBreakdown,
                RedTagStatusBreakdown = redTagStatusBreakdown,
                DailyAuditTrend = dailyAuditTrendRaw.Select(x => new DashboardTrendPointDto
                {
                    Label = x.Date.ToString("yyyy-MM-dd"),
                    Count = x.Count,
                    AveragePercentage = x.AveragePercentage
                }).ToList(),
                DailyRedTagTrend = dailyRedTagTrendRaw.Select(x => new DashboardTrendPointDto
                {
                    Label = x.Date.ToString("yyyy-MM-dd"),
                    Count = x.Count
                }).ToList()
            };
        }

        public async Task<AnalyticsAdvancedDashboardDto> GetAdvanced(int? companyId, int days)
        {
            var basic = await GetBasic(companyId, days);
            var effectiveDays = basic.PeriodDays;
            var fromDate = DateTime.UtcNow.AddDays(-effectiveDays);

            var auditsQuery = BuildAuditScopedQuery(companyId).Where(x => x.AuditDate >= fromDate);
            var redTagsQuery = BuildRedTagScopedQuery(companyId).Where(x => x.IdentifiedDate >= fromDate);

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

            var zonePerformance = zoneAgg
                .Select(x => new DashboardZoneInsightDto
                {
                    ZoneId = x.ZoneId,
                    ZoneName = zoneMap.TryGetValue(x.ZoneId, out var zoneName) ? zoneName : "Unknown",
                    AuditCount = x.AuditCount,
                    AveragePercentage = x.AveragePercentage
                })
                .OrderBy(x => x.AveragePercentage)
                .ToList();

            var departmentInsights = await auditsQuery
                .GroupBy(x => x.Department ?? "Unknown")
                .Select(g => new DepartmentInsightDto
                {
                    Department = g.Key,
                    AuditCount = g.Count(),
                    AveragePercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .OrderBy(x => x.AveragePercentage)
                .ToListAsync();

            var scoreBandInsights = new List<ScoreBandInsightDto>
            {
                new() { Band = "0-49", Count = await auditsQuery.CountAsync(x => x.Percentage < 50) },
                new() { Band = "50-69", Count = await auditsQuery.CountAsync(x => x.Percentage >= 50 && x.Percentage < 70) },
                new() { Band = "70-84", Count = await auditsQuery.CountAsync(x => x.Percentage >= 70 && x.Percentage < 85) },
                new() { Band = "85-100", Count = await auditsQuery.CountAsync(x => x.Percentage >= 85) }
            };

            var feedbackQuery = BuildFeedBackScopedQuery(companyId).Where(x => x.CreatedAt >= fromDate);

            var feedbackSentiment = new FeedbackSentimentInsightDto
            {
                GoodCount = await feedbackQuery.CountAsync(x => x.Good == true),
                BadCount = await feedbackQuery.CountAsync(x => x.Bad == true)
            };

            var lowPerformanceAudits = await auditsQuery
                .Where(x => x.Percentage < 70)
                .OrderBy(x => x.Percentage)
                .ThenByDescending(x => x.AuditDate)
                .Take(15)
                .Select(x => new DashboardRecentAuditDto
                {
                    Id = x.Id,
                    ZoneId = x.ZoneId,
                    AuditDate = x.AuditDate,
                    Percentage = x.Percentage,
                    Status = x.Status.ToString()
                })
                .ToListAsync();

            var avgClosureDays = await redTagsQuery
               .Where(r => r.ClosingDate != null)
                .Select(r => (double?)EF.Functions.DateDiffDay(r.IdentifiedDate, r.ClosingDate.Value))
                .AverageAsync();

            return new AnalyticsAdvancedDashboardDto
            {
                CompanyId = basic.CompanyId,
                PeriodDays = basic.PeriodDays,
                TotalAudits = basic.TotalAudits,
                AverageAuditPercentage = basic.AverageAuditPercentage,
                TotalRedTags = basic.TotalRedTags,
                OpenRedTags = basic.OpenRedTags,
                ClosedRedTags = basic.ClosedRedTags,
                AuditStatusBreakdown = basic.AuditStatusBreakdown,
                RedTagStatusBreakdown = basic.RedTagStatusBreakdown,
                DailyAuditTrend = basic.DailyAuditTrend,
                DailyRedTagTrend = basic.DailyRedTagTrend,
                ZonePerformance = zonePerformance,
                DepartmentInsights = departmentInsights,
                ScoreBandInsights = scoreBandInsights,
                FeedbackSentiment = feedbackSentiment,
                RecentLowPerformanceAudits = lowPerformanceAudits,
                AverageRedTagClosureDays =(decimal)avgClosureDays
            };
        }

        private IQueryable<Audit> BuildAuditScopedQuery(int? companyId)
        {
            var query = _auditRepo.Query();

            if (_currentUser.Role == "SuperAdmin")
            {
                if (companyId.HasValue)
                    query = query.Where(x => x.CompanyId == companyId.Value);

                return query;
            }

            return query.Where(x => x.CompanyId == _currentUser.CompanyId);
        }

        private IQueryable<RedTag> BuildRedTagScopedQuery(int? companyId)
        {
            var query = _redTagRepo.Query();

            if (_currentUser.Role == "SuperAdmin")
            {
                if (companyId.HasValue)
                    query = query.Where(x => x.CompanyId == companyId.Value);

                return query;
            }

            return query.Where(x => x.CompanyId == _currentUser.CompanyId);
        }

        private IQueryable<FeedBackItem> BuildFeedBackScopedQuery(int? companyId)
        {
            var query = _feedBackRepo.Query();

            if (_currentUser.Role == "SuperAdmin")
            {
                if (companyId.HasValue)
                    query = query.Where(x => x.CompanyId == companyId.Value);

                return query;
            }

            return query.Where(x => x.CompanyId == _currentUser.CompanyId);
        }

        private int? ResolveCompanyIdForResponse(int? companyId)
        {
            if (_currentUser.Role == "SuperAdmin")
                return companyId;

            return _currentUser.CompanyId;
        }
    }
}
