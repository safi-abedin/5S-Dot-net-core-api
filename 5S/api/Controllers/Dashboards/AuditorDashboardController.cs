using api.Services.Interfaces.Dashboards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Dashboards
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditorDashboardController : ControllerBase
    {
        private readonly IAuditorDashboardService _service;

        public AuditorDashboardController(IAuditorDashboardService service)
        {
            _service = service;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var result = await _service.GetByUserId(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }
    }
}
