using api.DTOS.RedTags;
using api.Helpers.Pagination;
using api.Models.RedTags;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.RedTags;
using api.Services.Interfaces.Users;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.RedTags
{
    public class RedTagService : IRedTagService
    {
        private readonly IRepository<RedTag> _repo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<RedTagService> _logger;

        public RedTagService(
            IRepository<RedTag> repo,
            ICurrentUserService currentUser,
            ILogger<RedTagService> logger)
        {
            _repo = repo;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<PagedResponse<RedTagResponseDto>> GetAll(PaginationRequest request)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query()
                .Where(x => x.CompanyId == companyId)
                .OrderByDescending(x => x.Id)
                .Select(x => new RedTagResponseDto
                {
                    Id = x.Id,
                    ItemName = x.ItemName,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    Location = x.Location,
                    PhotoUrl = x.PhotoUrl,
                    ResponsiblePerson = x.ResponsiblePerson,
                    Status = x.Status,
                    IdentifiedDate = x.IdentifiedDate,
                    ClosingDate = x.ClosingDate
                });

            _logger.LogInformation("Fetching red tags for CompanyId: {CompanyId}", companyId);

            return await PaginationHelper.CreateAsync(query, request.Page, request.Size);
        }

        public async Task<List<RedTagResponseDto>> GetAllByCompanyId(int companyId)
        {
            _logger.LogInformation("Fetching all red tags (without pagination) for CompanyId: {CompanyId}", companyId);

            return await _repo.Query()
                .Where(x => x.CompanyId == companyId)
                .OrderByDescending(x => x.Id)
                .Select(x => new RedTagResponseDto
                {
                    Id = x.Id,
                    ItemName = x.ItemName,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    Location = x.Location,
                    PhotoUrl = x.PhotoUrl,
                    ResponsiblePerson = x.ResponsiblePerson,
                    Status = x.Status,
                    IdentifiedDate = x.IdentifiedDate,
                    ClosingDate = x.ClosingDate
                })
                .ToListAsync();
        }

        public async Task<RedTagResponseDto> GetById(int id)
        {
            var companyId = _currentUser.CompanyId;

            var redTag = await _repo.Query()
                .Where(x => x.Id == id && x.CompanyId == companyId)
                .Select(x => new RedTagResponseDto
                {
                    Id = x.Id,
                    ItemName = x.ItemName,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    Location = x.Location,
                    PhotoUrl = x.PhotoUrl,
                    ResponsiblePerson = x.ResponsiblePerson,
                    Status = x.Status,
                    IdentifiedDate = x.IdentifiedDate,
                    ClosingDate = x.ClosingDate
                })
                .FirstOrDefaultAsync();

            if (redTag == null)
            {
                _logger.LogWarning("Red tag not found. Id: {Id}, CompanyId: {CompanyId}", id, companyId);
                throw new Exception("Red tag not found");
            }

            return redTag;
        }

        public async Task<int> Create(CreateRedTagDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemName))
                throw new Exception("Item name is required");

            var companyId = _currentUser.CompanyId;

            var entity = new RedTag
            {
                ItemName = dto.ItemName,
                Description = dto.Description,
                Quantity = dto.Quantity,
                Location = dto.Location,
                PhotoUrl = dto.PhotoUrl,
                ResponsiblePerson = dto.ResponsiblePerson,
                Status = dto.Status,
                IdentifiedDate = dto.IdentifiedDate ?? DateTime.UtcNow,
                ClosingDate = dto.ClosingDate,
                CompanyId = companyId
            };

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            _logger.LogInformation("Red tag created: {ItemName} for CompanyId: {CompanyId}", dto.ItemName, companyId);

            return entity.Id;
        }

        public async Task Update(UpdateRedTagDto dto)
        {
            var companyId = _currentUser.CompanyId;

            var redTag = await _repo.Query()
                .FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

            if (redTag == null)
                throw new Exception("Red tag not found");

            redTag.ItemName = dto.ItemName;
            redTag.Description = dto.Description;
            redTag.Quantity = dto.Quantity;
            redTag.Location = dto.Location;
            redTag.PhotoUrl = dto.PhotoUrl;
            redTag.ResponsiblePerson = dto.ResponsiblePerson;
            redTag.Status = dto.Status;
            redTag.IdentifiedDate = dto.IdentifiedDate;
            redTag.ClosingDate = dto.ClosingDate;

            _repo.Update(redTag);
            await _repo.SaveAsync();

            _logger.LogInformation("Red tag updated: {Id}", dto.Id);
        }

        public async Task Delete(int id)
        {
            var companyId = _currentUser.CompanyId;

            var redTag = await _repo.Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            if (redTag == null)
                throw new Exception("Red tag not found");

            _repo.Delete(redTag);
            await _repo.SaveAsync();

            _logger.LogWarning("Red tag deleted: {Id}", id);
        }
    }
}
