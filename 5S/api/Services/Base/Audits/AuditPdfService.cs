using api.DTOS.Audits;
using api.Enums;
using api.Helpers.PDF.Base;
using api.Models.Companies;
using api.Repositories.Interfaces.Base;
using api.Services.Interfaces.Audits;
using api.Services.Interfaces.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;

namespace api.Services.Base.Audits
{
    public class AuditPdfService : BasePDFService, IAuditPdfService
    {
        private readonly IAuditService _auditService;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuditPdfService> _logger;

        public AuditPdfService(
            IAuditService auditService,
            IRepository<Company> companyRepo,
            ICurrentUserService currentUser,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditPdfService> logger)
            : base(companyRepo, currentUser, httpClientFactory, environment, httpContextAccessor)
        {
            _auditService = auditService;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<byte[]> GenerateAsync(int id)
        {
            var audit = await _auditService.GetById(id);
            var company = await GetCurrentCompanyInfoAsync();
            var feedbacks = await BuildFeedbacksAsync(audit.FeedBackItems);
            var groupedItems = audit.Items
                .OrderBy(x => x.CatagoryOrder)
                .ThenBy(x => x.Order)
                .GroupBy(x => new { x.ChecklistCatagoryId, x.CatagoryName, x.CatagoryOrder })
                .Select(g => new AuditCategorySection
                {
                    CategoryId = g.Key.ChecklistCatagoryId ?? 0,
                    CategoryName = g.Key.CatagoryName,
                    CategoryOrder = g.Key.CatagoryOrder ?? 0,
                    AverageScore = g.Average(x => (decimal?)x.Score) ?? 0,
                    Items = g.OrderBy(x => x.Order).ToList()
                })
                .OrderBy(x => x.CategoryOrder)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(28);
                    page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10).FontColor(Colors.Grey.Darken4));

                    page.Header().Element(x => RenderHeader(x, company));
                    page.Content().Column(column =>
                    {
                        column.Spacing(14);
                        column.Item().Element(x => RenderTitle(x, audit));
                        column.Item().Element(x => RenderOverview(x, audit));
                        column.Item().Element(x => RenderItemsSection(x, groupedItems));
                        column.Item().Element(x => RenderFeedbackSection(x, feedbacks));
                    });
                    page.Footer().Element(RenderFooter);
                });
            });



            return document.GeneratePdf();
        }

        private static void RenderTitle(IContainer container, AuditResponseDto audit)
        {
            container.Column(column =>
            {
                column.Spacing(4);
                column.Item().Text("Audit Report").FontSize(15).Bold().FontColor(Colors.Black);
                column.Item().Text($"Generated on {DateTime.Now:dd MMM yyyy, hh:mm tt}").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        }

        private static void RenderOverview(IContainer container, AuditResponseDto audit)
        {
            container.Column(column =>
            {
                column.Spacing(12);
                column.Item().Row(row =>
                {
                    row.RelativeItem(2).Element(c => RenderScoreCard(c, audit));
                    row.RelativeItem(1).Element(c => RenderStatusCard(c, audit));
                });

                column.Item().Element(c => RenderInfoGrid(c, audit));
            });
        }

        private static void RenderScoreCard(IContainer container, AuditResponseDto audit)
        {
            container
                .Background(Colors.White)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(16)
                .Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text("Performance Summary").FontSize(13).Bold().FontColor(Colors.Grey.Darken3);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Spacing(2);
                            left.Item().Text("Total Score").FontSize(9).FontColor(Colors.Grey.Darken2);
                            left.Item().Text($"{audit.TotalScore:0.##}").FontSize(28).Bold().FontColor(Colors.Blue.Darken3);
                        });

                        row.RelativeItem().AlignRight().Column(right =>
                        {
                            right.Spacing(2);
                            right.Item().Text("Percentage").FontSize(9).FontColor(Colors.Grey.Darken2).AlignRight();
                            right.Item().Text($"{audit.Percentage:0.##}%").FontSize(28).Bold().FontColor(Colors.Green.Darken2).AlignRight();
                        });
                    });

                    var percent = (int)Math.Clamp(Math.Round(audit.Percentage), 0, 100);

                    column.Item().Column(progress =>
                    {
                        progress.Spacing(4);
                        progress.Item().Text("How well did this audit perform?").FontSize(9).FontColor(Colors.Grey.Darken2);
                        progress.Item().Height(12).Row(bar =>
                        {
                            bar.RelativeItem(percent).Background(Colors.Green.Medium);
                            bar.RelativeItem(100 - percent).Background(Colors.Grey.Lighten3);
                        });
                    });
                });
        }

        private static void RenderStatusCard(IContainer container, AuditResponseDto audit)
        {
            container
                .Background(Colors.White)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(16)
                .Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text("Audit Status").FontSize(10).Bold().FontColor(Colors.Grey.Darken3);
                    column.Item().Background(StatusColor(audit.Status)).PaddingVertical(8).PaddingHorizontal(9).AlignCenter().Text(FormatEnum(audit.Status)).Bold().FontSize(11).FontColor(Colors.White);
                    column.Item().Text($"Zone: {FormatLabel(audit.ZoneName)}").FontSize(9).Bold();
                    column.Item().Text($"Audit Date: {audit.AuditDate:dd MMM yyyy}").FontSize(9).Bold();
                });
        }

        private static void RenderInfoGrid(IContainer container, AuditResponseDto audit)
        {
            container
                .Background(Colors.White)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(16)
                .Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Text("Basic Information").FontSize(13).Bold().FontColor(Colors.Grey.Darken3);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Element(c => InfoTile(c, "Auditor", FormatLabel(audit.AuditorName)));
                        row.RelativeItem().Element(c => InfoTile(c, "Auditee", FormatLabel(audit.AuditeeName)));
                    });

                    column.Item().Row(row =>
                    {         
                        row.RelativeItem().Element(c => InfoTile(c, "Department", FormatLabel(audit.Department)));
                        row.RelativeItem().Element(c => InfoTile(c, "Created At", audit.CreatedAt.ToString("dd MMM yyyy, hh:mm tt")));
                    });

                });
        }

        private static void InfoTile(IContainer container, string label, string value)
        {
            container
                .Background(Colors.Grey.Lighten5)
                .Padding(10)
                .Column(column =>
                {
                    column.Spacing(2);
                    column.Item().Text(label).FontSize(7).FontColor(Colors.Grey.Darken2);
                    column.Item().Text(value).FontSize(9).Bold().FontColor(Colors.Grey.Darken4);
                });
        }

        private static void RenderItemsSection(IContainer container, List<AuditCategorySection> categories)
        {
            container
                .Background(Colors.White)
                .Padding(16)
                .Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text("Score by Category").FontSize(13).Bold().FontColor(Colors.Grey.Darken3);

                    if (!categories.Any())
                    {
                        column.Item().Text("No audit items available.").FontColor(Colors.Grey.Darken1);
                        return;
                    }

                    foreach (var category in categories)
                    {
                        column.Item().PaddingTop(6).Column(categoryColumn =>
                        {
                            categoryColumn.Spacing(6);
                            categoryColumn.Item().Background(Colors.Blue.Lighten5).Padding(8).Row(row =>
                            {
                                row.RelativeItem().Text($"{category.CategoryOrder}. {category.CategoryName}").Bold().FontColor(Colors.Blue.Darken3);
                                row.ConstantItem(110).AlignRight().Text($"Avg: {category.AverageScore:0.##}/5").Bold().FontColor(Colors.Blue.Darken3);
                            });

                            categoryColumn.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(44);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(5);
                                    columns.ConstantColumn(60);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(c => TableHeaderCell(c, "Order"));
                                    header.Cell().Element(c => TableHeaderCell(c, "Checking Item"));
                                    header.Cell().Element(c => TableHeaderCell(c, "Evaluation Criteria"));
                                    header.Cell().Element(c => TableHeaderCell(c, "Score"));
                                });

                                foreach (var item in category.Items)
                                {
                                    table.Cell().Element(c => TableDataCell(c)).Text(item.Order.ToString()).FontSize(9);
                                    table.Cell().Element(c => TableDataCell(c)).Text(FormatLabel(item.CheckingItemName)).FontSize(9);
                                    table.Cell().Element(c => TableDataCell(c)).Text(FormatLabel(item.EvaluationCriteria)).FontSize(9);
                                    table.Cell().Element(c => TableDataCell(c)).AlignCenter().Text(item.Score.ToString()).FontSize(9).Bold();
                                }
                            });
                        });
                    }
                });
        }

        private static void TableHeaderCell(IContainer container, string text)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Text(text)
                .SemiBold()
                .FontSize(9);
        }

        private static IContainer TableDataCell(IContainer container)
        {
            return container
                .PaddingVertical(6)
                .PaddingHorizontal(5)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3);
        }

        private static void RenderFeedbackSection(IContainer container, List<AuditFeedbackSection> feedbacks)
        {
            container
                .EnsureSpace(180)
                .Background(Colors.White)
                .Padding(16)
                .Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text("Feedback Items").FontSize(13).Bold().FontColor(Colors.Grey.Darken3);

                    if (!feedbacks.Any())
                    {
                        column.Item().Text("No feedback items available.").FontColor(Colors.Grey.Darken1);
                        return;
                    }

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(c => FeedbackHeaderCell(c, "Comment"));
                            header.Cell().Element(c => FeedbackHeaderCell(c, "Pictures"));
                        });

                        foreach (var feedback in feedbacks)
                        {
                            table.Cell().Element(c => FeedbackCommentCell(c, feedback));
                            table.Cell().Element(c => FeedbackImagesCell(c, feedback));
                        }
                    });
                });
        }

        private static void FeedbackHeaderCell(IContainer container, string text)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(8)
                .PaddingHorizontal(8)
                .Text(text)
                .SemiBold()
                .FontSize(10);
        }

        private static void FeedbackCommentCell(IContainer container, AuditFeedbackSection feedback)
        {
            var comment = string.IsNullOrWhiteSpace(feedback.Comment) ||
                          string.Equals(feedback.Comment?.Trim(), "N/A", StringComparison.OrdinalIgnoreCase)
                ? "No Comment for this feedback"
                : feedback.Comment;

            container
                .Background(FeedbackRowColor(feedback))
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(10)
                .AlignMiddle()
                .AlignLeft()
                .Text(comment)
                .FontSize(9)
                .FontColor(Colors.Grey.Darken3)
                .AlignCenter();
        }

        private static void FeedbackImagesCell(IContainer container, AuditFeedbackSection feedback)
        {
            container
                .Background(FeedbackRowColor(feedback))
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(8)
                .Column(column =>
                {
                    column.Spacing(6);

                    if (!feedback.ImageBytes.Any())
                    {
                        column.Item().AlignCenter().Text("No images").FontSize(9).FontColor(Colors.Grey.Darken2);
                        return;
                    }

                    column.Item().AlignCenter().Row(row =>
                    {
                        row.Spacing(4);

                        foreach (var image in feedback.ImageBytes.Take(5))
                        {
                            row.ConstantItem(60)
                                .Height(52)
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .Background(Colors.White)
                                .Padding(3)
                                .AlignLeft()
                                .AlignMiddle()
                                .Image(image, ImageScaling.FitArea);
                        }
                    });
                });
        }

        private static string FeedbackRowColor(AuditFeedbackSection feedback)
            => feedback.Good == true ? Colors.Green.Lighten5 : Colors.Red.Lighten5;

        private static string StatusColor(AuditStatus status) => status switch
        {
            AuditStatus.Draft => Colors.Grey.Darken1,
            AuditStatus.Submitted => Colors.Orange.Darken2,
            AuditStatus.Reviewed => Colors.Green.Darken2,
            _ => Colors.Grey.Darken2
        };

        private async Task<List<AuditFeedbackSection>> BuildFeedbacksAsync(List<FeedBackItemDto>? feedbackItems)
        {
            var result = new List<AuditFeedbackSection>();

            if (feedbackItems == null)
                return result;

            foreach (var feedback in feedbackItems)
            {
                var imageBytes = new List<byte[]>();

                if (feedback.ImageUrls != null)
                {
                    var downloads = feedback.ImageUrls
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(5)
                        .Select(ResolveFeedbackImageAsync)
                        .ToList();

                    var bytes = await Task.WhenAll(downloads);
                    imageBytes = bytes.Where(x => x is { Length: > 0 }).Select(x => x!).ToList();
                }

                result.Add(new AuditFeedbackSection
                {
                    Comment = feedback.Comment,
                    Good = feedback.Good,
                    Bad = feedback.Bad,
                    ImageBytes = imageBytes
                });
            }

            return result;
        }

        private async Task<byte[]?> ResolveFeedbackImageAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var absoluteUrl = url.Trim();

            if (Uri.TryCreate(absoluteUrl, UriKind.Relative, out _))
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                    absoluteUrl = $"{request.Scheme}://{request.Host}/{absoluteUrl.TrimStart('/')}";
            }

            var fromHttp = await TryDownloadImageAsync(absoluteUrl);
            if (fromHttp is { Length: > 0 })
                return fromHttp;

            var relativePath = url;
            if (Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
                relativePath = absoluteUri.AbsolutePath;

            var normalized = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            var fullPath = Path.Combine(webRoot, normalized);
            if (File.Exists(fullPath))
                return await File.ReadAllBytesAsync(fullPath);

            _logger.LogWarning("Feedback image could not be resolved. Url: {Url}, CheckedPath: {Path}", url, fullPath);
            return null;
        }

        private async Task<byte[]?> TryDownloadImageAsync(string absoluteUrl)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (AuditPdfService)");
                request.Headers.TryAddWithoutValidation("Accept", "image/*,*/*;q=0.8");

                using var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Image download failed with status {Status} for {Url}", response.StatusCode, absoluteUrl);
                    return null;
                }

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (mediaType != null && !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Image download returned non-image content type {ContentType} for {Url}", mediaType, absoluteUrl);
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error downloading feedback image from {Url}", absoluteUrl);
                return null;
            }
        }

        private sealed class AuditCategorySection
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public int CategoryOrder { get; set; }
            public decimal AverageScore { get; set; }
            public List<AuditItemDto> Items { get; set; } = new();
        }

        private sealed class AuditFeedbackSection
        {
            public string? Comment { get; set; }
            public bool? Good { get; set; }
            public bool? Bad { get; set; }
            public List<byte[]> ImageBytes { get; set; } = new();
        }
    }
}
