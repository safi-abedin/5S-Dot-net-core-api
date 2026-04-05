namespace api.DTOS.Companies
{
    public class CreateCompanyDto
    {
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
