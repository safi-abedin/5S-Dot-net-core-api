namespace api.DTOS.Dashboards
{
    public class AnalyticsBasicDashboardDto
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int? CompanyId { get; set; }
        public int PeriodDays { get; set; }
        public int TotalAudits { get; set; }
        public decimal AverageAuditPercentage { get; set; }
        public int TotalRedTags { get; set; }
        public int OpenRedTags { get; set; }
        public int ClosedRedTags { get; set; }
        public List<DashboardStatusCountDto> AuditStatusBreakdown { get; set; } = [];
        public List<DashboardStatusCountDto> RedTagStatusBreakdown { get; set; } = [];
        public List<DashboardTrendPointDto> DailyAuditTrend { get; set; } = [];
        public List<DashboardTrendPointDto> DailyRedTagTrend { get; set; } = [];
    }

    public class AnalyticsAdvancedDashboardDto : AnalyticsBasicDashboardDto
    {
        public List<DashboardZoneInsightDto> ZonePerformance { get; set; } = [];
        public List<DepartmentInsightDto> DepartmentInsights { get; set; } = [];
        public List<ScoreBandInsightDto> ScoreBandInsights { get; set; } = [];
        public FeedbackSentimentInsightDto FeedbackSentiment { get; set; } = new();
        public List<DashboardRecentAuditDto> RecentLowPerformanceAudits { get; set; } = [];
        public decimal? AverageRedTagClosureDays { get; set; }
    }

    public class DepartmentInsightDto
    {
        public string Department { get; set; } = string.Empty;
        public int AuditCount { get; set; }
        public decimal AveragePercentage { get; set; }
    }

    public class ScoreBandInsightDto
    {
        public string Band { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class FeedbackSentimentInsightDto
    {
        public int GoodCount { get; set; }
        public int BadCount { get; set; }
    }
}
