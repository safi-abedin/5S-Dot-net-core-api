using api.Services.Interfaces.Users;

namespace api.Services.Base.Users
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public int CompanyId =>
            int.Parse(_http.HttpContext.User.FindFirst("CompanyId")?.Value ?? "0");

        public int UserId =>
            int.Parse(_http.HttpContext.User.FindFirst("UserId")?.Value ?? "0");
    }
}
