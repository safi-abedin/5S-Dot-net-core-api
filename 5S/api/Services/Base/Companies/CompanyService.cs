using api.DTOS.Companies;
using api.Helpers.Pagination;
using api.Models.Checklists;
using api.Models.Companies;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Companies;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.Companies
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _repo;
        private readonly IRepository<ChecklistCategory> _categoryRepo;
        private readonly IRepository<ChecklistItem> _itemRepo;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(
            IRepository<Company> repo,
            IRepository<ChecklistCategory> categoryRepo,
            IRepository<ChecklistItem> itemRepo,
            ILogger<CompanyService> logger)
        {
            _repo = repo;
            _categoryRepo = categoryRepo;
            _itemRepo = itemRepo;
            _logger = logger;
        }

        public async Task<PagedResponse<CompanyResponseDto>> GetAll(PaginationRequest request)
        {
            var query = _repo.Query()
                .OrderByDescending(x => x.Id)
                .Select(x => new CompanyResponseDto
                {
                    Id = x.Id,
                    CompanyName = x.CompanyName,
                    CompanyCode = x.CompanyCode,
                    ContactPerson = x.ContactPerson,
                    Email = x.Email,
                    Phone = x.Phone
                });

            _logger.LogInformation("Fetching companies");

            return await PaginationHelper.CreateAsync(query, request.Page, request.Size);
        }

        public async Task<CompanyResponseDto> GetById(int id)
        {
            var company = await _repo.Query()
                .Where(x => x.Id == id)
                .Select(x => new CompanyResponseDto
                {
                    Id = x.Id,
                    CompanyName = x.CompanyName,
                    CompanyCode = x.CompanyCode,
                    ContactPerson = x.ContactPerson,
                    Email = x.Email,
                    Phone = x.Phone
                })
                .FirstOrDefaultAsync();

            if (company == null)
            {
                _logger.LogWarning("Company not found. Id: {Id}", id);
                throw new Exception("Company not found");
            }

            return company;
        }

        public async Task<int> Create(CreateCompanyDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                throw new Exception("Company name is required");

            var duplicate = await _repo.Query().AnyAsync(x => x.CompanyCode == dto.CompanyCode);
            if (duplicate)
                throw new Exception("Company code already exists");

            var entity = new Company
            {
                CompanyName = dto.CompanyName,
                CompanyCode = dto.CompanyCode,
                ContactPerson = dto.ContactPerson,
                Email = dto.Email,
                Phone = dto.Phone
            };

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            await SeedChecklistForCompany(entity.Id);

            _logger.LogInformation("Company created: {CompanyName}", dto.CompanyName);

            return entity.Id;
        }

        public async Task Update(UpdateCompanyDto dto)
        {
            var company = await _repo.Query()
                .FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (company == null)
                throw new Exception("Company not found");

            var duplicate = await _repo.Query().AnyAsync(x => x.CompanyCode == dto.CompanyCode && x.Id != dto.Id);
            if (duplicate)
                throw new Exception("Company code already exists");

            company.CompanyName = dto.CompanyName;
            company.CompanyCode = dto.CompanyCode;
            company.ContactPerson = dto.ContactPerson;
            company.Email = dto.Email;
            company.Phone = dto.Phone;

            _repo.Update(company);
            await _repo.SaveAsync();

            _logger.LogInformation("Company updated: {Id}", dto.Id);
        }

        public async Task Delete(int id)
        {
            var company = await _repo.Query()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (company == null)
                throw new Exception("Company not found");

            _repo.Delete(company);
            await _repo.SaveAsync();

            _logger.LogWarning("Company deleted: {Id}", id);
        }

        private async Task SeedChecklistForCompany(int companyId)
        {
            var templateCategories = await _categoryRepo.Query()
                .Where(x => x.CompanyId == null)
                .OrderBy(x => x.Order)
                .ToListAsync();

            if (!templateCategories.Any())
            {
                _logger.LogWarning("Checklist template categories not found. CompanyId: {CompanyId}", companyId);
                return;
            }

            var clonedCategories = templateCategories
                .Select(x => new ChecklistCategory
                {
                    Name = x.Name,
                    Order = x.Order,
                    CompanyId = companyId
                })
                .ToList();

            foreach (var category in clonedCategories)
                await _categoryRepo.AddAsync(category);

            await _categoryRepo.SaveAsync();

            var categoryMap = templateCategories
                .Select((template, index) => new { template.Id, NewId = clonedCategories[index].Id })
                .ToDictionary(x => x.Id, x => x.NewId);

            var templateItems = await _itemRepo.Query()
                .Where(x => x.CompanyId == null)
                .OrderBy(x => x.CategoryId)
                .ThenBy(x => x.Order)
                .ToListAsync();

            var clonedItems = templateItems
                .Where(x => categoryMap.ContainsKey(x.CategoryId))
                .Select(x => new ChecklistItem
                {
                    CategoryId = categoryMap[x.CategoryId],
                    CheckingItemName = x.CheckingItemName,
                    EvaluationCriteria = x.EvaluationCriteria,
                    MaxScore = x.MaxScore,
                    Order = x.Order,
                    CompanyId = companyId
                })
                .ToList();

            foreach (var item in clonedItems)
                await _itemRepo.AddAsync(item);

            await _itemRepo.SaveAsync();

            _logger.LogInformation("Checklist template seeded for CompanyId: {CompanyId}. Categories: {CategoryCount}, Items: {ItemCount}", companyId, clonedCategories.Count, clonedItems.Count);
        }
    }
}
