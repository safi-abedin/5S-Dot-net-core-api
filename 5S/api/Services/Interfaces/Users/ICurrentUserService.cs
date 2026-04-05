namespace api.Services.Interfaces.Users
{
    public interface ICurrentUserService
    {
        int CompanyId { get; }
        int UserId { get; }
        string Role { get; }
    }
}
