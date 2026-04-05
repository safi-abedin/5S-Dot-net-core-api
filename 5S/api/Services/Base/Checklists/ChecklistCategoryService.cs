using api.DTOS.Checklists;
using api.Models.Checklists;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Checklists;
using api.Services.Interfaces.Users;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.Checklists
{
    public class ChecklistCategoryService : IChecklistCategoryService
    {
        private readonly IRepository<ChecklistCategory> _repo;
        private readonly ICurrentUserService _currentUser;

        public ChecklistCategoryService(
            IRepository<ChecklistCategory> repo,
            ICurrentUserService currentUser)
        {
            _repo = repo;
            _currentUser = currentUser;
        }

        public async Task<List<ChecklistCategoryResponseDto>> GetAll()
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query();

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId || x.CompanyId == null);
            }

            return await query
                .OrderBy(x => x.Order)
                .Select(x => new ChecklistCategoryResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Order = x.Order
                })
                .ToListAsync();
        }
    }
}
