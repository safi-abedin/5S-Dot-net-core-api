using api.DTOS.Checklists;
using api.Helpers.Pagination;
using api.Services.Interfaces.Checklists;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Checklists
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChecklistController : ControllerBase
    {
        private readonly IChecklistService _service;

        public ChecklistController(IChecklistService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
        {
            var result = await _service.GetAll(request);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllWithoutPagination()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(int categoryId)
        {
            var result = await _service.GetByCategoryId(categoryId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetById(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateChecklistDto dto)
        {
            var id = await _service.Create(dto);
            return Ok(new { Id = id });
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update(UpdateChecklistDto dto)
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
