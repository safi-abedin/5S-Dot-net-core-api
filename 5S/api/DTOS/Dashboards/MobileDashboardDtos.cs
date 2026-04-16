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
        public int Id { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public DateTime AuditDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MobileDashboardRecentTagDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ResponsiblePerson { get; set; } = string.Empty;
        public DateTime IdentifiedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class MobileDashboardZonePerformanceDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public int TotalAudits { get; set; }
        public decimal ScorePercentage { get; set; }
    }

    public class MobileDashboardPerformanceDto
    {
        public decimal HighestScore { get; set; }
        public decimal LowestScore { get; set; }
        public decimal AverageScore { get; set; }
    }
}
