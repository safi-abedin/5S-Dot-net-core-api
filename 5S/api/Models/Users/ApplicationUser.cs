using api.Models.Companies;
using Microsoft.AspNetCore.Identity;

namespace api.Models.Users
{
    public class ApplicationUser : IdentityUser<int>
    {
        public int? CompanyId { get; set; }
        public Company Company { get; set; }

        public string Name { get; set; }
    }
}
