namespace api.DTOS.Dashboards
{
    public class AuditorDashboardResponseDto
    {
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int TotalAudits { get; set; }
        public int CompletedAudits { get; set; }
        public decimal AverageAuditPercentage { get; set; }
        public int TotalRedTags { get; set; }
        public int OpenRedTags { get; set; }
        public int ClosedRedTags { get; set; }
        public decimal? AverageRedTagClosureDays { get; set; }
        public List<DashboardStatusCountDto> AuditStatusBreakdown { get; set; } = [];
        public List<DashboardTrendPointDto> MonthlyAuditTrend { get; set; } = [];
        public List<DashboardZoneInsightDto> ZoneInsights { get; set; } = [];
        public List<DashboardRecentAuditDto> RecentAudits { get; set; } = [];
    }

    public class DashboardStatusCountDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DashboardTrendPointDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal? AveragePercentage { get; set; }
    }

    public class DashboardZoneInsightDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public int AuditCount { get; set; }
        public decimal AveragePercentage { get; set; }
    }

    public class DashboardRecentAuditDto
    {
        public int Id { get; set; }
        public int ZoneId { get; set; }
        public DateTime AuditDate { get; set; }
        public decimal Percentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
