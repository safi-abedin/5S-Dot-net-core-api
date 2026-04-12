namespace api.Helpers.PDF.Models
{
    public class CompanyPdfInfo
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public byte[]? LogoBytes { get; set; }
    }
}
