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
            try
            {
                var result = await _service.GetAllByCompanyId(companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RedTagPaginationRequest request)
        {
            try
            {
                var result = await _service.GetAll(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await _service.GetById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateRedTagDto dto)
        {
            try
            {
                var id = await _service.Create(dto);
                return Ok(new { Id = id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpPost("update")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update([FromForm] UpdateRedTagDto dto)
        {
            try
            {
                await _service.Update(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            try
            {
                await _service.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }
    }
}
