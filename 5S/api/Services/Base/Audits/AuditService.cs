using api.DTOS.Audits;
using api.Helpers.Pagination;
using api.Models.Audits;
using api.Models.Checklists;
using api.Models.Zones;
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
        private readonly IRepository<Zone> _zoneRepo;
        private readonly IRepository<ChecklistItem> _checklistRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AuditService> _logger;
        private readonly IFileStorageService _fileStorage;

        public AuditService(
            IRepository<Audit> repo,
            IRepository<Zone> zoneRepo,
            IRepository<ChecklistItem> checklistRepo,
            ICurrentUserService currentUser,
            ILogger<AuditService> logger,
            IFileStorageService fileStorage)
        {
            _repo = repo;
            _zoneRepo = zoneRepo;
            _checklistRepo = checklistRepo;
            _currentUser = currentUser;
            _logger = logger;
            _fileStorage = fileStorage;
        }

        public async Task<PagedResponse<AuditResponseDto>> GetAll(AuditPaginationRequest request)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query().AsNoTracking();
            var zoneQuery = _zoneRepo.Query().AsNoTracking();

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
                zoneQuery = zoneQuery.Where(z => z.CompanyId == companyId);
            }

            if (request.ZoneId.HasValue)
            {
                query = query.Where(x => x.ZoneId == request.ZoneId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.AuditorName))
            {
                query = query.Where(x => x.AuditorName.Contains(request.AuditorName));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            if (request.MinScore.HasValue)
            {
                query = query.Where(x => x.TotalScore >= request.MinScore.Value);
            }

            if (request.MaxScore.HasValue)
            {
                query = query.Where(x => x.TotalScore <= request.MaxScore.Value);
            }

            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= request.CreatedTo.Value);
            }

            if (request.AuditDateFrom.HasValue)
            {
                query = query.Where(x => x.AuditDate >= request.AuditDateFrom.Value);
            }

            if (request.AuditDateTo.HasValue)
            {
                query = query.Where(x => x.AuditDate <= request.AuditDateTo.Value);
            }

            query = ApplySorting(query, request);

            var projectedQuery = query.Select(x => new AuditResponseDto
            {
                Id = x.Id,
                ZoneId = x.ZoneId,
                ZoneName = zoneQuery
                    .Where(z => z.Id == x.ZoneId)
                    .Select(z => z.Name)
                    .FirstOrDefault() ?? string.Empty,
                AuditorName = x.AuditorName,
                AuditeeName = x.AuditeeName,
                Department = x.Department,
                AuditDate = x.AuditDate,
                CreatedAt = x.CreatedAt,
                TotalScore = x.TotalScore,
                Percentage = x.Percentage,
                Status = x.Status,
                Items = x.Items.Select(i => new AuditItemDto
                {
                    Id = i.Id,
                    ChecklistItemId = i.ChecklistItemId,
                    ChecklistCatagoryId = i.ChecklistItem.CategoryId,
                    CatagoryName = i.ChecklistItem.Category.Name,
                    CatagoryOrder = i.ChecklistItem.Category.Order,
                    CheckingItemName = i.ChecklistItem.CheckingItemName,
                    EvaluationCriteria = i.ChecklistItem.EvaluationCriteria,
                    Order = i.ChecklistItem.Order,
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

        private static IQueryable<Audit> ApplySorting(IQueryable<Audit> query, BasePaginationRequest request)
        {
            var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return request.SortBy?.Trim().ToLowerInvariant() switch
            {
                "zoneid" => descending ? query.OrderByDescending(x => x.ZoneId) : query.OrderBy(x => x.ZoneId),
                "auditorname" => descending ? query.OrderByDescending(x => x.AuditorName) : query.OrderBy(x => x.AuditorName),
                "totalscore" => descending ? query.OrderByDescending(x => x.TotalScore) : query.OrderBy(x => x.TotalScore),
                "percentage" => descending ? query.OrderByDescending(x => x.Percentage) : query.OrderBy(x => x.Percentage),
                "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                "auditdate" => descending ? query.OrderByDescending(x => x.AuditDate) : query.OrderBy(x => x.AuditDate),
                "createdat" => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };
        }

        public async Task<AuditResponseDto> GetById(int id)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query().AsNoTracking().Where(x => x.Id == id);
            var zoneQuery = _zoneRepo.Query().AsNoTracking();

            if (_currentUser.Role != "SuperAdmin")
            {
                query = query.Where(x => x.CompanyId == companyId);
                zoneQuery = zoneQuery.Where(z => z.CompanyId == companyId);
            }

            var audit = await query
                .Select(x => new AuditResponseDto
                {
                    Id = x.Id,
                    ZoneId = x.ZoneId,
                    ZoneName = zoneQuery
                        .Where(z => z.Id == x.ZoneId)
                        .Select(z => z.Name)
                        .FirstOrDefault() ?? string.Empty,
                    AuditorName = x.AuditorName,
                    AuditeeName = x.AuditeeName,
                    Department = x.Department,
                    AuditDate = x.AuditDate,
                    CreatedAt = x.CreatedAt,
                    TotalScore = x.TotalScore,
                    Percentage = x.Percentage,
                    Status = x.Status,
                    Items = x.Items.Select(i => new AuditItemDto
                    {
                        Id = i.Id,
                        ChecklistItemId = i.ChecklistItemId,
                        ChecklistCatagoryId = i.ChecklistItem.CategoryId,
                        CatagoryName = i.ChecklistItem.Category.Name,
                        CatagoryOrder = i.ChecklistItem.Category.Order,
                        CheckingItemName = i.ChecklistItem.CheckingItemName,
                        EvaluationCriteria = i.ChecklistItem.EvaluationCriteria,
                        Order = i.ChecklistItem.Order,
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

            var (items, totalScore, percentage) = await BuildAuditItemsAndScoresAsync(dto.Items, companyId);

            var entity = new Audit
            {
                ZoneId = dto.ZoneId,
                AuditorName = dto.AuditorName,
                AuditeeName = dto.AuditeeName,
                Department = dto.Department,
                AuditDate = dto.AuditDate,
                TotalScore = totalScore,
                Percentage = percentage,
                Status = dto.Status,
                CompanyId = companyId,
                Items = items,
                FeedBackItems = feedBackItems?.ToList(),
                CreatedBy = _currentUser.UserId
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

            var (items, totalScore, percentage) = await BuildAuditItemsAndScoresAsync(dto.Items, audit.CompanyId ?? companyId);

            audit.ZoneId = dto.ZoneId;
            audit.AuditorName = dto.AuditorName;
            audit.AuditeeName = dto.AuditeeName;
            audit.Department = dto.Department;
            audit.AuditDate = dto.AuditDate;
            audit.TotalScore = totalScore;
            audit.Percentage = percentage;
            audit.Status = dto.Status;
            audit.LastUpdatedAt = DateTime.UtcNow;

            if (audit.Items.Any())
                audit.Items.Clear();

            foreach (var item in items)
            {
                audit.Items.Add(item);
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
                var feedbackItems = new List<FeedBackItem>();

                foreach (var feedBackItem in dto.FeedBackItems)
                {
                    feedbackItems.Add(new FeedBackItem
                    {
                        Comment = feedBackItem.Comment,
                        ImageUrls = await _fileStorage.SaveManyAsync(feedBackItem.Images, "audit-feedback"),
                        Good = feedBackItem.Good,
                        Bad = feedBackItem.Bad,
                        CompanyId = audit.CompanyId
                    });
                }

                audit.FeedBackItems = feedbackItems;
            }

            _repo.Update(audit);
            await _repo.SaveAsync();

            _logger.LogInformation("Audit updated: {Id}", dto.Id);
        }

        private async Task<(List<AuditItem> Items, decimal TotalScore, decimal Percentage)> BuildAuditItemsAndScoresAsync(
            IEnumerable<AuditItemDto>? submittedItems,
            int companyId)
        {
            var checklistQuery = _checklistRepo.Query().AsNoTracking();

            checklistQuery = checklistQuery.Where(x => x.CompanyId == companyId);

            var checklistItems = await checklistQuery
                .Select(x => new { x.Id, x.CategoryId })
                .ToListAsync();

            if (!checklistItems.Any())
                throw new Exception("Checklist items not found");

            var submittedMap = (submittedItems ?? [])
                .GroupBy(x => x.ChecklistItemId)
                .ToDictionary(g => g.Key, g => g.First().Score);

            var normalizedItems = checklistItems
                .Select(ci => new
                {
                    ci.Id,
                    ci.CategoryId,
                    Score = submittedMap.TryGetValue(ci.Id, out var score) ? score : 5
                })
                .ToList();

            var categoryScores = normalizedItems
                .GroupBy(x => x.CategoryId)
                .Select(g => g.Average(x => (decimal)x.Score))
                .ToList();

            var obtainedScore = categoryScores.Sum();
            var maxPossibleScore = categoryScores.Count * 5m;
            var percentage = maxPossibleScore == 0 ? 0 : Math.Round((obtainedScore / maxPossibleScore) * 100m, 2);

            var auditItems = normalizedItems
                .Select(x => new AuditItem
                {
                    ChecklistItemId = x.Id,
                    Score = x.Score,
                    CompanyId = companyId
                })
                .ToList();

            return (auditItems, percentage, percentage);
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
