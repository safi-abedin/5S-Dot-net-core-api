using api.Services.Interfaces.Checklists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Checklists
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChecklistCategoryController : ControllerBase
    {
        private readonly IChecklistCategoryService _service;

        public ChecklistCategoryController(IChecklistCategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }
    }
}
