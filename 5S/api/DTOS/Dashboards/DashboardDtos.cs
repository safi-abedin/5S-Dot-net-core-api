namespace api.DTOS.Dashboards
{
    public class DashboardSummaryDto
    {
        public int TotalAudits { get; set; }
        public decimal AveragePercentage { get; set; }
        public int TotalRedTags { get; set; }
        public int OpenRedTags { get; set; }
        public int ClosedRedTags { get; set; }
    }

    public class DashboardTrendDto
    {
        public DateTime Date { get; set; }
        public int AuditCount { get; set; }
        public decimal AvgPercentage { get; set; }
    }

    public class DashboardCategoryScoreDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
    }

    public class DashboardZonePerformanceDto
    {
        public string ZoneName { get; set; } = string.Empty;
        public decimal AveragePercentage { get; set; }
    }

    public class DashboardTopPerformerDto
    {
        public string UserName { get; set; } = string.Empty;
        public decimal AveragePercentage { get; set; }
    }

    public class WebDashboardRecentAuditDto
    {
        public string AuditorName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public DateTime AuditDate { get; set; }
    }

    public class DashboardFeedbackSummaryDto
    {
        public int TotalFeedbacks { get; set; }
        public int GoodCount { get; set; }
        public int BadCount { get; set; }
    }
}
