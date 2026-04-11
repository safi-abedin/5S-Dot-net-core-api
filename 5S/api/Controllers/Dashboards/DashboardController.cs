using api.Data;
using api.DTOS.Dashboards;
using api.Enums;
using api.Models.Audits;
using api.Models.RedTags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers.Dashboards
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int companyId, [FromQuery] int? userId, [FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);
            var auditsQuery = BuildAuditQuery(companyId, userId, from, toExclusive);
            var redTagsQuery = BuildRedTagQuery(companyId, userId, from, toExclusive);

            var totalAudits = await auditsQuery.CountAsync();
            var averagePercentage = await auditsQuery.Select(x => (decimal?)x.Percentage).AverageAsync() ?? 0;

            var totalRedTags = await redTagsQuery.CountAsync();
            var closedRedTags = await redTagsQuery.CountAsync(x => x.Status == RedTagStatus.Closed);
            var openRedTags = totalRedTags - closedRedTags;

            return Ok(new DashboardSummaryDto
            {
                TotalAudits = totalAudits,
                AveragePercentage = averagePercentage,
                TotalRedTags = totalRedTags,
                OpenRedTags = openRedTags,
                ClosedRedTags = closedRedTags
            });
        }

        [HttpGet("trend")]
        public async Task<IActionResult> GetTrend([FromQuery] int companyId, [FromQuery] int? userId, [FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var data = await BuildAuditQuery(companyId, userId, from, toExclusive)
                .GroupBy(x => x.AuditDate.Date)
                .Select(g => new DashboardTrendDto
                {
                    Date = g.Key,
                    AuditCount = g.Count(),
                    AvgPercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("category-scores")]
        public async Task<IActionResult> GetCategoryScores([FromQuery] int companyId, [FromQuery] int? userId, [FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);
            var auditIdsQuery = BuildAuditQuery(companyId, userId, from, toExclusive)
                .Select(x => x.Id);

            var data = await _db.AuditItems.AsNoTracking()
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
                .Select(g => new DashboardCategoryScoreDto
                {
                    CategoryName = g.Key,
                    AverageScore = g.Average(x => (decimal?)x.Score) ?? 0
                })
                .OrderBy(x => x.CategoryName)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("zone-performance")]
        public async Task<IActionResult> GetZonePerformance([FromQuery] int companyId, [FromQuery] int? userId, [FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var data = await BuildAuditQuery(companyId, userId, from, toExclusive)
                .Join(_db.Zones.AsNoTracking().Where(x => !x.IsDeleted),
                    a => a.ZoneId,
                    z => z.Id,
                    (a, z) => new { z.Name, a.Percentage })
                .GroupBy(x => x.Name)
                .Select(g => new DashboardZonePerformanceDto
                {
                    ZoneName = g.Key,
                    AveragePercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .OrderByDescending(x => x.AveragePercentage)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("top-performers")]
        public async Task<IActionResult> GetTopPerformers([FromQuery] int companyId, [FromQuery] int? userId, [FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var data = await BuildAuditQuery(companyId, userId, from, toExclusive)
                .Join(_db.Users.AsNoTracking(),
                    a => a.CreatedBy,
                    u => u.Id,
                    (a, u) => new { u.Name, a.Percentage })
                .GroupBy(x => x.Name)
                .Select(g => new DashboardTopPerformerDto
                {
                    UserName = g.Key ?? string.Empty,
                    AveragePercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .OrderByDescending(x => x.AveragePercentage)
                .Take(5)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("recent-audits")]
        public async Task<IActionResult> GetRecentAudits([FromQuery] int companyId, [FromQuery] int? userId, [FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var data = await BuildAuditQuery(companyId, userId, from, toExclusive)
                .Join(_db.Zones.AsNoTracking().Where(z => !z.IsDeleted),
                    a => a.ZoneId,
                    z => z.Id,
                    (a, z) => new { a, z.Name })
                .OrderByDescending(x => x.a.AuditDate)
                .Take(10)
                .Select(x => new WebDashboardRecentAuditDto
                {
                    AuditorName = x.a.AuditorName,
                    Department = x.a.Department,
                    ZoneName = x.Name,
                    Percentage = x.a.Percentage,
                    AuditDate = x.a.AuditDate
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("feedback-summary")]
        public async Task<IActionResult> GetFeedbackSummary([FromQuery] int companyId, [FromQuery] int? userId, [FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);
            var auditIdsQuery = BuildAuditQuery(companyId, userId, from, toExclusive)
                .Select(x => x.Id);

            var feedbackQuery = _db.Set<FeedBackItem>().AsNoTracking()
                .Where(x => !x.IsDeleted && x.CreatedAt >= from && x.CreatedAt < toExclusive && auditIdsQuery.Contains(x.AuditId));

            var totalFeedbacks = await feedbackQuery.CountAsync();
            var goodCount = await feedbackQuery.CountAsync(x => x.Good == true);
            var badCount = await feedbackQuery.CountAsync(x => x.Bad == true);

            return Ok(new DashboardFeedbackSummaryDto
            {
                TotalFeedbacks = totalFeedbacks,
                GoodCount = goodCount,
                BadCount = badCount
            });
        }

        private IQueryable<Audit> BuildAuditQuery(int companyId, int? userId, DateTime from, DateTime toExclusive)
        {
            var query = _db.Audits.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == companyId && x.AuditDate >= from && x.AuditDate < toExclusive);

            if (userId.HasValue)
                query = query.Where(x => x.CreatedBy == userId.Value);

            return query;
        }

        private IQueryable<RedTag> BuildRedTagQuery(int companyId, int? userId, DateTime from, DateTime toExclusive)
        {
            var query = _db.RedTags.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CompanyId == companyId && x.CreatedAt >= from && x.CreatedAt < toExclusive);

            if (userId.HasValue)
                query = query.Where(x => x.CreatedBy == userId.Value);

            return query;
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
    }
}
