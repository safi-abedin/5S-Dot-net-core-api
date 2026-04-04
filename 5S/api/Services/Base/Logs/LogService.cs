using api.Data;
using api.Models.Logs;
using api.Services.Interfaces.Logs;

namespace api.Services.Base.Logs
{
    public class LogService : ILogService
    {
        private readonly AppDbContext _context;

        public LogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string message, string level, int? companyId, int? userId)
        {
            var log = new AppLog
            {
                Message = message,
                Level = level,
                CompanyId = companyId,
                UserId = userId,
                TimeStamp = DateTime.UtcNow
            };

            await _context.AppLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
