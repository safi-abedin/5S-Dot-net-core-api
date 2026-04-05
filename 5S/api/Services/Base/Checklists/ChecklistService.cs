using api.DTOS.Checklists;
using api.Helpers.Pagination;
using api.Models.Checklists;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Checklists;
using api.Services.Interfaces.Users;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace api.Services.Base.Checklists
{
    public class ChecklistService : IChecklistService
    {
        private readonly IRepository<ChecklistItem> _repo;
        private readonly IRepository<ChecklistCategory> _categoryRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ChecklistService> _logger;

        public ChecklistService(
            IRepository<ChecklistItem> repo,
            IRepository<ChecklistCategory> categoryRepo,
            ICurrentUserService currentUser,
            ILogger<ChecklistService> logger)
        {
            _repo = repo;
            _categoryRepo = categoryRepo;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<PagedResponse<ChecklistResponseDto>> GetAll(PaginationRequest request)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query();

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var projectedQuery = query
                .OrderBy(x => x.CategoryId)
                .ThenBy(x => x.Order)
                .Select(x => new ChecklistResponseDto
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    CheckingItemName = x.CheckingItemName,
                    EvaluationCriteria = x.EvaluationCriteria,
                    MaxScore = x.MaxScore,
                    Order = x.Order
                });

            _logger.LogInformation("Fetching checklists for CompanyId: {CompanyId}", companyId);

            return await PaginationHelper.CreateAsync(projectedQuery, request.Page, request.Size);
        }

        public async Task<List<ChecklistResponseDto>> GetAll()
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query();

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            return await query
                .OrderBy(x => x.CategoryId)
                .ThenBy(x => x.Order)
                .Select(x => new ChecklistResponseDto
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    CheckingItemName = x.CheckingItemName,
                    EvaluationCriteria = x.EvaluationCriteria,
                    MaxScore = x.MaxScore,
                    Order = x.Order
                })
                .ToListAsync();
        }

        public async Task<List<ChecklistResponseDto>> GetByCategoryId(int categoryId)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query().Where(x => x.CategoryId == categoryId);

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            return await query
                .OrderBy(x => x.Order)
                .Select(x => new ChecklistResponseDto
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    CheckingItemName = x.CheckingItemName,
                    EvaluationCriteria = x.EvaluationCriteria,
                    MaxScore = x.MaxScore,
                    Order = x.Order
                })
                .ToListAsync();
        }

        public async Task<ChecklistResponseDto> GetById(int id)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query().Where(x => x.Id == id);

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var checklist = await query
                .Select(x => new ChecklistResponseDto
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    CheckingItemName = x.CheckingItemName,
                    EvaluationCriteria = x.EvaluationCriteria,
                    MaxScore = x.MaxScore,
                    Order = x.Order
                })
                .FirstOrDefaultAsync();

            if (checklist == null)
            {
                _logger.LogWarning("Checklist not found. Id: {Id}, CompanyId: {CompanyId}", id, companyId);
                throw new Exception("Checklist not found");
            }

            return checklist;
        }

        public async Task<int> Create(CreateChecklistDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CheckingItemName))
                throw new Exception("Checking item name is required");

            var companyId = _currentUser.CompanyId;

            var categoryExists = await _categoryRepo.Query()
                .AnyAsync(x => x.Id == dto.CategoryId && x.CompanyId == companyId);

            if (!categoryExists)
                throw new Exception("Checklist category not found");

            var entity = new ChecklistItem
            {
                CategoryId = dto.CategoryId,
                CheckingItemName = dto.CheckingItemName,
                EvaluationCriteria = dto.EvaluationCriteria,
                MaxScore = dto.MaxScore,
                Order = dto.Order,
                CompanyId = companyId
            };

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            _logger.LogInformation("Checklist item created: {Name} for CompanyId: {CompanyId}", dto.CheckingItemName, companyId);

            return entity.Id;
        }

        public async Task Update(UpdateChecklistDto dto)
        {
            var companyId = _currentUser.CompanyId;

            var checklist = await _repo.Query()
                .FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

            if (checklist == null)
                throw new Exception("Checklist not found");

            var categoryExists = await _categoryRepo.Query()
                .AnyAsync(x => x.Id == dto.CategoryId && x.CompanyId == companyId);

            if (!categoryExists)
                throw new Exception("Checklist category not found");

            checklist.CategoryId = dto.CategoryId;
            checklist.CheckingItemName = dto.CheckingItemName;
            checklist.EvaluationCriteria = dto.EvaluationCriteria;
            checklist.MaxScore = dto.MaxScore;
            checklist.Order = dto.Order;

            _repo.Update(checklist);
            await _repo.SaveAsync();

            _logger.LogInformation("Checklist item updated: {Id}", dto.Id);
        }

        public async Task Delete(int id)
        {
            var companyId = _currentUser.CompanyId;

            var checklist = await _repo.Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            if (checklist == null)
                throw new Exception("Checklist not found");

            _repo.Delete(checklist);
            await _repo.SaveAsync();

            _logger.LogWarning("Checklist item deleted: {Id}", id);
        }
    }
}
