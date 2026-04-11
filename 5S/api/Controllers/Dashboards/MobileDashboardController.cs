using api.Data;
using api.DTOS.Dashboards;
using api.Enums;
using api.Models.Audits;
using api.Models.RedTags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api.Controllers.Dashboards
{
    [ApiController]
    [Route("api/mobile-dashboard")]
    [Authorize]
    public class MobileDashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MobileDashboardController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userId = GetLoggedInUserId();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var auditsQuery = BuildAuditQuery(userId, from, toExclusive);
            var redTagsQuery = BuildRedTagQuery(userId, from, toExclusive);

            var totalAudits = await auditsQuery.CountAsync();
            var averagePercentage = await auditsQuery.Select(x => (decimal?)x.Percentage).AverageAsync() ?? 0;

            var totalRedTags = await redTagsQuery.CountAsync();
            var closedRedTags = await redTagsQuery.CountAsync(x => x.Status == RedTagStatus.Closed);
            var openRedTags = totalRedTags - closedRedTags;

            return Ok(new MobileDashboardSummaryDto
            {
                TotalAudits = totalAudits,
                AveragePercentage = averagePercentage,
                TotalRedTags = totalRedTags,
                OpenRedTags = openRedTags,
                ClosedRedTags = closedRedTags
            });
        }

        [HttpGet("trend")]
        public async Task<IActionResult> GetTrend([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userId = GetLoggedInUserId();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var trend = await BuildAuditQuery(userId, from, toExclusive)
                .GroupBy(x => x.AuditDate.Date)
                .Select(g => new MobileDashboardTrendDto
                {
                    Date = g.Key,
                    AuditCount = g.Count(),
                    AvgPercentage = g.Average(x => (decimal?)x.Percentage) ?? 0
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(trend);
        }

        [HttpGet("category-scores")]
        public async Task<IActionResult> GetCategoryScores([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userId = GetLoggedInUserId();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var auditIdsQuery = BuildAuditQuery(userId, from, toExclusive).Select(x => x.Id);

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
                .Select(g => new MobileDashboardCategoryScoreDto
                {
                    CategoryName = g.Key,
                    AverageScore = g.Average(x => (decimal?)x.Score) ?? 0
                })
                .OrderBy(x => x.CategoryName)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("recent-audits")]
        public async Task<IActionResult> GetRecentAudits([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userId = GetLoggedInUserId();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);

            var data = await BuildAuditQuery(userId, from, toExclusive)
                .OrderByDescending(x => x.AuditDate)
                .Take(10)
                .Select(x => new MobileDashboardRecentAuditDto
                {
                    AuditDate = x.AuditDate,
                    Department = x.Department,
                    Percentage = x.Percentage,
                    Status = x.Status.ToString()
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformance([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userId = GetLoggedInUserId();
            var (from, toExclusive) = ResolveDateRange(days, fromDate, toDate);
            var auditsQuery = BuildAuditQuery(userId, from, toExclusive);

            var hasData = await auditsQuery.AnyAsync();

            if (!hasData)
            {
                return Ok(new MobileDashboardPerformanceDto
                {
                    HighestScore = 0,
                    LowestScore = 0,
                    AverageScore = 0
                });
            }

            var highest = await auditsQuery.MaxAsync(x => (decimal?)x.Percentage) ?? 0;
            var lowest = await auditsQuery.MinAsync(x => (decimal?)x.Percentage) ?? 0;
            var average = await auditsQuery.AverageAsync(x => (decimal?)x.Percentage) ?? 0;

            return Ok(new MobileDashboardPerformanceDto
            {
                HighestScore = highest,
                LowestScore = lowest,
                AverageScore = average
            });
        }

        private IQueryable<Audit> BuildAuditQuery(int userId, DateTime from, DateTime toExclusive)
        {
            return _db.Audits.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CreatedBy == userId && x.AuditDate >= from && x.AuditDate < toExclusive);
        }

        private IQueryable<RedTag> BuildRedTagQuery(int userId, DateTime from, DateTime toExclusive)
        {
            return _db.RedTags.AsNoTracking()
                .Where(x => !x.IsDeleted && x.CreatedBy == userId && x.CreatedAt >= from && x.CreatedAt < toExclusive);
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

        private int GetLoggedInUserId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? User.FindFirstValue("UserId");

            if (!int.TryParse(claimValue, out var userId))
                throw new UnauthorizedAccessException("Invalid user claim");

            return userId;
        }
    }
}
