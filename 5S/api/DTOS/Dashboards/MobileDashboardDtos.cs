namespace api.DTOS.Dashboards
{
    public class MobileDashboardSummaryDto
    {
        public int TotalAudits { get; set; }
        public decimal AveragePercentage { get; set; }
        public int TotalRedTags { get; set; }
        public int OpenRedTags { get; set; }
        public int ClosedRedTags { get; set; }
    }

    public class MobileDashboardTrendDto
    {
        public DateTime Date { get; set; }
        public int AuditCount { get; set; }
        public decimal AvgPercentage { get; set; }
    }

    public class MobileDashboardCategoryScoreDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
    }

    public class MobileDashboardRecentAuditDto
    {
        public DateTime AuditDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MobileDashboardPerformanceDto
    {
        public decimal HighestScore { get; set; }
        public decimal LowestScore { get; set; }
        public decimal AverageScore { get; set; }
    }
}
