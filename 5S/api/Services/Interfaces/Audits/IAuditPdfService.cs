namespace api.Services.Interfaces.Audits
{
    public interface IAuditPdfService
    {
        Task<byte[]> GenerateAsync(int id);
    }
}
