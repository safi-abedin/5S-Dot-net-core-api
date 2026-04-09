using api.Services.Interfaces.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Dashboards
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsDashboardController : ControllerBase
    {
        private readonly IAnalyticsDashboardService _service;

        public AnalyticsDashboardController(IAnalyticsDashboardService service)
        {
            _service = service;
        }

        [HttpGet("basic")]
        public async Task<IActionResult> GetBasic([FromQuery] int? companyId, [FromQuery] int days = 30)
        {
            try
            {
                var result = await _service.GetBasic(companyId, days);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("advanced")]
        public async Task<IActionResult> GetAdvanced([FromQuery] int? companyId, [FromQuery] int days = 90)
        {
            try
            {
                var result = await _service.GetAdvanced(companyId, days);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }
    }
}
