using api.Services.Interfaces.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Dashboards
{
    [ApiController]
    [Route("api/mobile-dashboard")]
    [Authorize]
    public class MobileDashboardController : ControllerBase
    {
        private readonly IMobileDashboardService _service;

        public MobileDashboardController(IMobileDashboardService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.GetSummary(days, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("trend")]
        public async Task<IActionResult> GetTrend([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.GetTrend(days, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("category-scores")]
        public async Task<IActionResult> GetCategoryScores([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.GetCategoryScores(days, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("recent-audits")]
        public async Task<IActionResult> GetRecentAudits([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.GetRecentAudits(days, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("recent-tags")]
        public async Task<IActionResult> GetRecentTags([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.GetRecentTags(days, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("zone-performance")]
        public async Task<IActionResult> GetZonePerformance([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.GetZonePerformance(days, fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformance([FromQuery] int? days = 30, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.GetPerformance(days, fromDate, toDate);
            return Ok(result);
        }
    }
}
