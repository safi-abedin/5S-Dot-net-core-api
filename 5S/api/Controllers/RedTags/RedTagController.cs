using api.DTOS.RedTags;
using api.Helpers.Pagination;
using api.Services.Interfaces.RedTags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.RedTags
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RedTagController : ControllerBase
    {
        private readonly IRedTagService _service;

        public RedTagController(IRedTagService service)
        {
            _service = service;
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetAllByCompanyId(int companyId)
        {
            var result = await _service.GetAllByCompanyId(companyId);
            return Ok(result);
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
        public async Task<IActionResult> Create(CreateRedTagDto dto)
        {
            var id = await _service.Create(dto);
            return Ok(new { Id = id });
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update(UpdateRedTagDto dto)
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
