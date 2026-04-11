using api.DTOS.RedTags;
using api.Helpers.Pagination;
using api.Models.RedTags;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.RedTags;
using api.Services.Interfaces.Users;
using api.Services.Interfaces.Files;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.RedTags
{
    public class RedTagService : IRedTagService
    {
        private readonly IRepository<RedTag> _repo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<RedTagService> _logger;
        private readonly IFileStorageService _fileStorage;

        public RedTagService(
            IRepository<RedTag> repo,
            ICurrentUserService currentUser,
            ILogger<RedTagService> logger,
            IFileStorageService fileStorage)
        {
            _repo = repo;
            _currentUser = currentUser;
            _logger = logger;
            _fileStorage = fileStorage;
        }

        public async Task<PagedResponse<RedTagResponseDto>> GetAll(RedTagPaginationRequest request)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query()
                .Where(x => x.CompanyId == companyId);

            if (!string.IsNullOrWhiteSpace(request.ResponsiblePerson))
            {
                query = query.Where(x => x.ResponsiblePerson.Contains(request.ResponsiblePerson));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            if (request.IdentifiedDateFrom.HasValue)
            {
                query = query.Where(x => x.IdentifiedDate >= request.IdentifiedDateFrom.Value);
            }

            if (request.IdentifiedDateTo.HasValue)
            {
                query = query.Where(x => x.IdentifiedDate <= request.IdentifiedDateTo.Value);
            }

            if (request.ClosingDateFrom.HasValue)
            {
                query = query.Where(x => x.ClosingDate.HasValue && x.ClosingDate.Value >= request.ClosingDateFrom.Value);
            }

            if (request.ClosingDateTo.HasValue)
            {
                query = query.Where(x => x.ClosingDate.HasValue && x.ClosingDate.Value <= request.ClosingDateTo.Value);
            }

            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= request.CreatedTo.Value);
            }

            query = ApplySorting(query, request);

            var projectedQuery = query.Select(x => new RedTagResponseDto
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
                ClosingDate = x.ClosingDate,
                CreatedAt = x.CreatedAt
            });

            _logger.LogInformation("Fetching red tags for CompanyId: {CompanyId}", companyId);

            return await PaginationHelper.CreateAsync(projectedQuery, request.Page, request.Size);
        }

        private static IQueryable<RedTag> ApplySorting(IQueryable<RedTag> query, BasePaginationRequest request)
        {
            var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return request.SortBy?.Trim().ToLowerInvariant() switch
            {
                "responsibleperson" => descending ? query.OrderByDescending(x => x.ResponsiblePerson) : query.OrderBy(x => x.ResponsiblePerson),
                "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                "identifieddate" => descending ? query.OrderByDescending(x => x.IdentifiedDate) : query.OrderBy(x => x.IdentifiedDate),
                "closingdate" => descending ? query.OrderByDescending(x => x.ClosingDate) : query.OrderBy(x => x.ClosingDate),
                "createdat" => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };
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
                    ClosingDate = x.ClosingDate,
                    CreatedAt = x.CreatedAt
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
                    ClosingDate = x.ClosingDate,
                    CreatedAt = x.CreatedAt
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
            var photoUrls = await _fileStorage.SaveManyAsync(dto.Photos, "red-tags");

            var entity = new RedTag
            {
                ItemName = dto.ItemName,
                Description = dto.Description,
                Quantity = dto.Quantity,
                Location = dto.Location,
                PhotoUrl = photoUrls,
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

            var photoUrls = await _fileStorage.SaveManyAsync(dto.Photos, "red-tags");

            redTag.ItemName = dto.ItemName;
            redTag.Description = dto.Description;
            redTag.Quantity = dto.Quantity;
            redTag.Location = dto.Location;
            redTag.PhotoUrl = photoUrls;
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
