using api.Helpers.PDF.Models;
using api.Models.Companies;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Users;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

namespace api.Helpers.PDF.Base
{
    public abstract class BasePDFService
    {
        private readonly IRepository<Company> _companyRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IHttpClientFactory _httpClientFactory;

        protected BasePDFService(IRepository<Company> companyRepo, ICurrentUserService currentUser, IHttpClientFactory httpClientFactory)
        {
            _companyRepo = companyRepo;
            _currentUser = currentUser;
            _httpClientFactory = httpClientFactory;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        protected async Task<CompanyPdfInfo> GetCurrentCompanyInfoAsync()
        {
            var company = await _companyRepo.Query()
                .Where(x => x.Id == _currentUser.CompanyId)
                .Select(x => new CompanyPdfInfo
                {
                    Id = x.Id,
                    CompanyName = x.CompanyName,
                    CompanyAddress = x.CompanyAddress,
                    LogoUrl = x.LogoUrl,
                    ContactPerson = x.ContactPerson,
                    Email = x.Email,
                    Phone = x.Phone
                })
                .FirstOrDefaultAsync();

            if (company == null)
                throw new Exception("Company not found");

            company.LogoBytes = await DownloadImageAsync(company.LogoUrl);
            return company;
        }

        protected async Task<byte[]?> DownloadImageAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            try
            {
                return await _httpClientFactory.CreateClient().GetByteArrayAsync(url);
            }
            catch
            {
                return null;
            }
        }

        protected static string FormatLabel(string? value) => string.IsNullOrWhiteSpace(value) ? "N/A" : value;

        protected static string FormatEnum<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var text = value.ToString().Replace("_", " ");
            return Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
        }

        protected static void RenderHeader(IContainer container, CompanyPdfInfo company)
        {
            container.PaddingBottom(12).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Row(row =>
            {
                row.ConstantItem(74).Height(74).Padding(6).AlignCenter().AlignMiddle().Element(box =>
                {
                    if (company.LogoBytes is { Length: > 0 })
                    {
                        box.Image(Image.FromBinaryData(company.LogoBytes));
                        return;
                    }

                    box.Background(Colors.Blue.Lighten4).AlignCenter().AlignMiddle().Text(GetInitials(company.CompanyName)).FontSize(18).SemiBold().FontColor(Colors.Blue.Darken3);
                });

                row.RelativeItem().PaddingLeft(12).PaddingTop(12).Column(column =>
                {
                    column.Spacing(2);
                    column.Item().Text(company.CompanyName).FontSize(18).SemiBold().FontColor(Colors.Blue.Darken3);
                    column.Item().Text(FormatLabel(company.CompanyAddress)).FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                    column.Item().Text($"Email: {FormatLabel(company.Email)} | Phone: {FormatLabel(company.Phone)}").FontSize(9).FontColor(Colors.Grey.Darken1).Bold();
                });
            });
        }

        protected static void RenderFooter(IContainer container)
        {
            container
                .PaddingTop(8)
                .BorderTop(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Column(column =>
                {
                    column.Item().PaddingTop(12).AlignCenter().Text("* This document is System Generated.").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
        }

        private static string GetInitials(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "C";

            var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (words.Length == 1)
                return words[0].Length > 1 ? words[0][..2].ToUpperInvariant() : words[0].ToUpperInvariant();

            return string.Concat(words.Take(2).Select(x => x[0])).ToUpperInvariant();
        }
    }
}
