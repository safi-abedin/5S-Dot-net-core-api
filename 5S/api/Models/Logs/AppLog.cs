namespace api.Models.Logs
{
    public class AppLog
    {
        public int Id { get; set; }

        public string Message { get; set; }
        public string Level { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Exception { get; set; }

        public int? UserId { get; set; }
        public int? CompanyId { get; set; }

        public string RequestPath { get; set; }
        public string ActionName { get; set; }
    }
}
