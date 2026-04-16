using api.Data;
using api.DTOS.Dashboards;
using api.Enums;
using api.Models.Audits;
using api.Models.RedTags;
using api.Models.Zones;
using api.Services.Interfaces.Dashboards;
using api.Services.Interfaces.Users;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.Dashboards
{
    public class MobileDashboardService : IMobileDashboardService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public MobileDashboardService(AppDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<MobileDashboardSummaryDto> GetSummary(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var (userId, companyId) = GetScope();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var auditsQuery = BuildAuditQuery(userId, companyId, from, toExclusive);
            var redTagsQuery = BuildRedTagQuery(userId, companyId, from, toExclusive);

            var totalAudits = await auditsQuery.CountAsync();
            var averagePercentage = await auditsQuery.Select(x => (decimal?)x.Percentage).AverageAsync() ?? 0;

            var totalRedTags = await redTagsQuery.CountAsync();
            var closedRedTags = await redTagsQuery.CountAsync(x => x.Status == RedTagStatus.Closed);
            var openRedTags = totalRedTags - closedRedTags;

            return new MobileDashboardSummaryDto
            {
                TotalAudits = totalAudits,
                AveragePercentage = averagePercentage,
                TotalRedTags = totalRedTags,
                OpenRedTags = openRedTags,
                ClosedRedTags = closedRedTags
            };
        }

        public async Task<List<MobileDashboardTrendDto>> GetTrend(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var (userId, companyId) = GetScope();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            return await BuildAuditQuery(userId, companyId, from, toExclusive)
                .GroupBy(x => x.AuditDate.Date)
                .Select(g => new MobileDashboardTrendDto
                {
                    Date = g.Key,
                    AuditCount = g.Count(),
                    AvgPercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<MobileDashboardCategoryScoreDto>> GetCategoryScores(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var (userId, companyId) = GetScope();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var auditIdsQuery = BuildAuditQuery(userId, companyId, from, toExclusive).Select(x => x.Id);

            return await _db.AuditItems.AsNoTracking()
                .Where(x => !x.IsDeleted && auditIdsQuery.Contains(x.AuditId))
                .Join(_db.ChecklistItems.AsNoTracking().Where(x => !x.IsDeleted),
                    ai => ai.ChecklistItemId,
                    ci => ci.Id,
                    (ai, ci) => new { ai.Score, ci.CategoryId })
                .Join(_db.ChecklistCategories.AsNoTracking().Where(x => !x.IsDeleted),
                    x => x.CategoryId,
                    c => c.Id,
                    (x, c) => new { c.Name, x.Score })
                .GroupBy(x => x.Name)
                .Select(g => new MobileDashboardCategoryScoreDto
                {
                    CategoryName = g.Key,
                    AverageScore = g.Average(x => (decimal?)x.Score) ?? 0
                })
                .OrderBy(x => x.CategoryName)
                .ToListAsync();
        }

        public async Task<List<MobileDashboardRecentAuditDto>> GetRecentAudits(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var (userId, companyId) = GetScope();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            return await BuildAuditQuery(userId, companyId, from, toExclusive)
                .Join(_db.Zones.AsNoTracking().Where(z => !z.IsDeleted),
                    a => a.ZoneId,
                    z => z.Id,
                    (a, z) => new { Audit = a, ZoneName = z.Name })
                .OrderByDescending(x => x.Audit.AuditDate)
                .Take(10)
                .Select(x => new MobileDashboardRecentAuditDto
                {
                    Id = x.Audit.Id,
                    ZoneName = x.ZoneName,
                    AuditDate = x.Audit.AuditDate,
                    Department = x.Audit.Department,
                    Percentage = x.Audit.Percentage,
                    Status = x.Audit.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task<MobileDashboardPerformanceDto> GetPerformance(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var (userId, companyId) = GetScope();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);
            var auditsQuery = BuildAuditQuery(userId, companyId, from, toExclusive);

            var hasData = await auditsQuery.AnyAsync();

            if (!hasData)
            {
                return new MobileDashboardPerformanceDto
                {
                    HighestScore = 0,
                    LowestScore = 0,
                    AverageScore = 0
                };
            }

            var highest = await auditsQuery.MaxAsync(x => (decimal?)x.Percentage) ?? 0;
            var lowest = await auditsQuery.MinAsync(x => (decimal?)x.Percentage) ?? 0;
            var average = await auditsQuery.AverageAsync(x => (decimal?)x.Percentage) ?? 0;

            return new MobileDashboardPerformanceDto
            {
                HighestScore = highest,
                LowestScore = lowest,
                AverageScore = average
            };
        }

        public async Task<List<MobileDashboardRecentTagDto>> GetRecentTags(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var (userId, companyId) = GetScope();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            return await BuildRedTagQuery(userId, companyId, from, toExclusive)
                .OrderByDescending(x => x.IdentifiedDate)
                .Take(10)
                .Select(x => new MobileDashboardRecentTagDto
                {
                    Id = x.Id,
                    ItemName = x.ItemName,
                    Location = x.Location,
                    ResponsiblePerson = x.ResponsiblePerson,
                    IdentifiedDate = x.IdentifiedDate,
                    Status = x.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task<List<MobileDashboardZonePerformanceDto>> GetZonePerformance(int? days = 30, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var (userId, companyId) = GetScope();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            return await BuildAuditQuery(userId, companyId, from, toExclusive)
                .GroupBy(x => x.ZoneId)
                .Select(g => new
                {
                    ZoneId = g.Key,
                    TotalAudits = g.Count(),
                    AverageScore = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .Join(_db.Zones.AsNoTracking().Where(z => !z.IsDeleted),
                    x => x.ZoneId,
                    z => z.Id,
                    (x, z) => new MobileDashboardZonePerformanceDto
                    {
                        ZoneId = z.Id,
                        ZoneName = z.Name,
                        TotalAudits = x.TotalAudits,
                        ScorePercentage = x.AverageScore
                    })
                .OrderByDescending(x => x.ScorePercentage)
                .ToListAsync();
        }

        private IQueryable<Audit> BuildAuditQuery(int userId, int companyId, DateTime from, DateTime toExclusive)
        {
            var query = _db.Audits.AsNoTracking()
                .Where(x => !x.IsDeleted && x.AuditDate >= from && x.AuditDate < toExclusive);


            if (userId > 0)
                return query.Where(x => x.CreatedBy == userId);

            return query.Where(_ => false);
        }

        private IQueryable<RedTag> BuildRedTagQuery(int userId, int companyId, DateTime from, DateTime toExclusive)
        {
            var query = _db.RedTags.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IdentifiedDate >= from && x.IdentifiedDate < toExclusive);


            if (userId > 0)
                return query.Where(x => x.CreatedBy == userId);

            return query.Where(_ => false);
        }

        private static (DateTime From, DateTime ToExclusive) ResolveDateRange(int? days, DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue || toDate.HasValue)
            {
                var from = (fromDate ?? DateTime.UtcNow.Date.AddDays(-(days is > 0 ? days.Value - 1 : 29))).Date;
                var to = (toDate ?? DateTime.UtcNow.Date).Date.AddDays(1);
                return (from, to);
            }

            var effectiveDays = days is > 0 ? days.Value : 30;
            var start = DateTime.UtcNow.Date.AddDays(-(effectiveDays - 1));
            var endExclusive = DateTime.UtcNow.Date.AddDays(1);
            return (start, endExclusive);
        }

        private (int UserId, int CompanyId) GetScope()
        {
            return (_currentUser.UserId, _currentUser.CompanyId);
        }
    }
}
