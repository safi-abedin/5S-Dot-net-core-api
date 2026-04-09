using api.DTOS.Audits;
using api.Helpers.Pagination;
using api.Models.Audits;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Audits;
using api.Services.Interfaces.Users;
using Microsoft.EntityFrameworkCore;
using api.Services.Interfaces.Files;

namespace api.Services.Base.Audits
{
    public class AuditService : IAuditService
    {
        private readonly IRepository<Audit> _repo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AuditService> _logger;
        private readonly IFileStorageService _fileStorage;

        public AuditService(
            IRepository<Audit> repo,
            ICurrentUserService currentUser,
            ILogger<AuditService> logger,
            IFileStorageService fileStorage)
        {
            _repo = repo;
            _currentUser = currentUser;
            _logger = logger;
            _fileStorage = fileStorage;
        }

        public async Task<PagedResponse<AuditResponseDto>> GetAll(PaginationRequest request)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query().AsNoTracking();

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var projectedQuery = query
                .OrderByDescending(x => x.Id)
                .Select(x => new AuditResponseDto
                {
                    Id = x.Id,
                    ZoneId = x.ZoneId,
                    AuditorName = x.AuditorName,
                    AuditeeName = x.AuditeeName,
                    Department = x.Department,
                    AuditDate = x.AuditDate,
                    TotalScore = x.TotalScore,
                    Percentage = x.Percentage,
                    Status = x.Status,
                    Items = x.Items.Select(i => new AuditItemDto
                    {
                        Id = i.Id,
                        ChecklistItemId = i.ChecklistItemId,
                        Score = i.Score
                    }).ToList(),
                    FeedBackItems = x.FeedBackItems == null
                        ? null
                        : x.FeedBackItems.Select(f => new FeedBackItemDto
                        {
                            Id = f.Id,
                            Comment = f.Comment,
                            ImageUrls = f.ImageUrls,
                            Good = f.Good,
                            Bad = f.Bad
                        }).ToList()
                });

            _logger.LogInformation("Fetching audits for CompanyId: {CompanyId}", companyId);

            return await PaginationHelper.CreateAsync(projectedQuery, request.Page, request.Size);
        }

        public async Task<AuditResponseDto> GetById(int id)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query().AsNoTracking().Where(x => x.Id == id);

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var audit = await query
                .Select(x => new AuditResponseDto
                {
                    Id = x.Id,
                    ZoneId = x.ZoneId,
                    AuditorName = x.AuditorName,
                    AuditeeName = x.AuditeeName,
                    Department = x.Department,
                    AuditDate = x.AuditDate,
                    TotalScore = x.TotalScore,
                    Percentage = x.Percentage,
                    Status = x.Status,
                    Items = x.Items.Select(i => new AuditItemDto
                    {
                        Id = i.Id,
                        ChecklistItemId = i.ChecklistItemId,
                        Score = i.Score
                    }).ToList(),
                    FeedBackItems = x.FeedBackItems == null
                        ? null
                        : x.FeedBackItems.Select(f => new FeedBackItemDto
                        {
                            Id = f.Id,
                            Comment = f.Comment,
                            ImageUrls = f.ImageUrls,
                            Good = f.Good,
                            Bad = f.Bad
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (audit == null)
            {
                _logger.LogWarning("Audit not found. Id: {Id}, CompanyId: {CompanyId}", id, companyId);
                throw new Exception("Audit not found");
            }

            return audit;
        }

        public async Task<int> Create(CreateAuditDto dto)
        {
            var companyId = _currentUser.CompanyId;

            var feedBackItems = dto.FeedBackItems == null
                ? null
                : await Task.WhenAll(dto.FeedBackItems.Select(async f => new FeedBackItem
                {
                    Comment = f.Comment,
                    ImageUrls = await _fileStorage.SaveManyAsync(f.Images, "audit-feedback"),
                    Good = f.Good,
                    Bad = f.Bad,
                    CompanyId = companyId
                }));

            var entity = new Audit
            {
                ZoneId = dto.ZoneId,
                AuditorName = dto.AuditorName,
                AuditeeName = dto.AuditeeName,
                Department = dto.Department,
                AuditDate = dto.AuditDate,
                TotalScore = dto.TotalScore,
                Percentage = dto.Percentage,
                Status = dto.Status,
                CompanyId = companyId,
                Items = dto.Items.Select(i => new AuditItem
                {
                    ChecklistItemId = i.ChecklistItemId,
                    Score = i.Score,
                    CompanyId = companyId
                }).ToList(),
                FeedBackItems = feedBackItems?.ToList()
            };

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            _logger.LogInformation("Audit created: {Id} for CompanyId: {CompanyId}", entity.Id, companyId);

            return entity.Id;
        }

        public async Task Update(UpdateAuditDto dto)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query()
                .Include(x => x.Items)
                .Include(x => x.FeedBackItems)
                .Where(x => x.Id == dto.Id);

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var audit = await query.FirstOrDefaultAsync();

            if (audit == null)
                throw new Exception("Audit not found");

            audit.ZoneId = dto.ZoneId;
            audit.AuditorName = dto.AuditorName;
            audit.AuditeeName = dto.AuditeeName;
            audit.Department = dto.Department;
            audit.AuditDate = dto.AuditDate;
            audit.TotalScore = dto.TotalScore;
            audit.Percentage = dto.Percentage;
            audit.Status = dto.Status;
            audit.LastUpdatedAt = DateTime.UtcNow;

            if (audit.Items.Any())
                audit.Items.Clear();

            foreach (var item in dto.Items)
            {
                audit.Items.Add(new AuditItem
                {
                    ChecklistItemId = item.ChecklistItemId,
                    Score = item.Score,
                    CompanyId = audit.CompanyId
                });
            }

            if (audit.FeedBackItems != null)
            {
                if (audit.FeedBackItems.Any())
                    audit.FeedBackItems.Clear();

                if (dto.FeedBackItems != null)
                {
                    foreach (var feedBackItem in dto.FeedBackItems)
                    {
                        audit.FeedBackItems.Add(new FeedBackItem
                        {
                            Comment = feedBackItem.Comment,
                            ImageUrls = await _fileStorage.SaveManyAsync(feedBackItem.Images, "audit-feedback"),
                            Good = feedBackItem.Good,
                            Bad = feedBackItem.Bad,
                            CompanyId = audit.CompanyId
                        });
                    }
                }
            }
            else if (dto.FeedBackItems != null)
            {
                var items = new List<FeedBackItem>();

                foreach (var feedBackItem in dto.FeedBackItems)
                {
                    items.Add(new FeedBackItem
                    {
                        Comment = feedBackItem.Comment,
                        ImageUrls = await _fileStorage.SaveManyAsync(feedBackItem.Images, "audit-feedback"),
                        Good = feedBackItem.Good,
                        Bad = feedBackItem.Bad,
                        CompanyId = audit.CompanyId
                    });
                }

                audit.FeedBackItems = items;
            }

            _repo.Update(audit);
            await _repo.SaveAsync();

            _logger.LogInformation("Audit updated: {Id}", dto.Id);
        }

        public async Task Delete(int id)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query().Where(x => x.Id == id);

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var audit = await query.FirstOrDefaultAsync();

            if (audit == null)
                throw new Exception("Audit not found");

            _repo.Delete(audit);
            await _repo.SaveAsync();

            _logger.LogWarning("Audit deleted: {Id}", id);
        }
    }
}
