using api.DTOS.Audits;
using api.Helpers.Pagination;
using api.Services.Interfaces.Audits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Audits
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _service;

        public AuditController(IAuditService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
        {
            var result = await _service.GetAll(request);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetById(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateAuditDto dto)
        {
            var id = await _service.Create(dto);
            return Ok(new { Id = id });
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateAuditDto dto)
        {
            await _service.Update(dto);
            return Ok();
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            await _service.Delete(id);
            return Ok();
        }
    }
}
