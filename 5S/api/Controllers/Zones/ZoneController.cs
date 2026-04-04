using api.Models.Zones;
using api.Services.Interfaces.Zones;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Zones
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZoneController : ControllerBase
    {
        private readonly IZoneService _service;

        public ZoneController(IZoneService service)
        {
            _service = service;
        }

        private int GetCompanyId()
        {
            return int.Parse(User.FindFirst("CompanyId")?.Value ?? "0");
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _service.GetAll(GetCompanyId());
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Zone zone)
        {
            zone.CompanyId = GetCompanyId();
            await _service.Create(zone);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Delete(id, GetCompanyId());
            return Ok();
        }
    }
}
