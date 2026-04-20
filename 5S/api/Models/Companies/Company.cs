using api.Models.Base;

namespace api.Models.Companies
{
    public class Company : BaseEntity
    {
        public string CompanyName { get; set; }

        public string? CompanyAddress { get; set; }

        public string CompanyCode { get; set; } // login key
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public string? LogoUrl { get; set; } = string.Empty;
    }
}
