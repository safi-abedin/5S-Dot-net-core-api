using Microsoft.AspNetCore.Http;

namespace api.DTOS.Profiles
{
    public class UpdateCompanyBrandingDto
    {
        public string CompanyAddress { get; set; } = string.Empty;
        public IFormFile? Logo { get; set; }
    }
}
