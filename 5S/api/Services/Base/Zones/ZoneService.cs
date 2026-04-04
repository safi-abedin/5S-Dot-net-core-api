using api.DTOS.Zones;
using api.Helpers.Pagination;
using api.Models.Zones;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Users;
using api.Services.Interfaces.Zones;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Base.Zones
{
    public class ZoneService : IZoneService
    {
        private readonly IRepository<Zone> _repo;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ZoneService> _logger;

        public ZoneService(
            IRepository<Zone> repo,
            ICurrentUserService currentUser,
            ILogger<ZoneService> logger)
        {
            _repo = repo;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<PagedResponse<ZoneResponseDto>> GetAll(PaginationRequest request)
        {
            var companyId = _currentUser.CompanyId;

            var query = _repo.Query()
                .Where(x => x.CompanyId == companyId)
                .OrderByDescending(x => x.Id)
                .Select(x => new ZoneResponseDto
                {
                    Id = x.Id,
                    Name = x.Name
                });

            _logger.LogInformation("Fetching zones for CompanyId: {CompanyId}", companyId);

            return await PaginationHelper.CreateAsync(query, request.Page, request.Size);
        }

        public async Task<ZoneResponseDto> GetById(int id)
        {
            var companyId = _currentUser.CompanyId;

            var zone = await _repo.Query()
                .Where(x => x.Id == id && x.CompanyId == companyId)
                .Select(x => new ZoneResponseDto
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefaultAsync();

            if (zone == null)
            {
                _logger.LogWarning("Zone not found. Id: {Id}, CompanyId: {CompanyId}", id, companyId);
                throw new Exception("Zone not found");
            }

            return zone;
        }

        public async Task<int> Create(CreateZoneDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Zone name is required");

            var companyId = _currentUser.CompanyId;

            var entity = new Zone
            {
                Name = dto.Name,
                CompanyId = companyId
            };

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            _logger.LogInformation("Zone created: {Name} for CompanyId: {CompanyId}", dto.Name, companyId);

            return entity.Id;
        }

        public async Task Update(UpdateZoneDto dto)
        {
            var companyId = _currentUser.CompanyId;

            var zone = await _repo.Query()
                .FirstOrDefaultAsync(x => x.Id == dto.Id && x.CompanyId == companyId);

            if (zone == null)
                throw new Exception("Zone not found");

            zone.Name = dto.Name;

            _repo.Update(zone);
            await _repo.SaveAsync();

            _logger.LogInformation("Zone updated: {Id}", dto.Id);
        }

        public async Task Delete(int id)
        {
            var companyId = _currentUser.CompanyId;

            var zone = await _repo.Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);

            if (zone == null)
                throw new Exception("Zone not found");

            _repo.Delete(zone);
            await _repo.SaveAsync();

            _logger.LogWarning("Zone deleted: {Id}", id);
        }
    }
}
