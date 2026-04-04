namespace api.Services.Interfaces.Logs
{
    public interface ILogService
    {
        Task LogAsync(string message, string level, int? companyId, int? userId);
    }
}
