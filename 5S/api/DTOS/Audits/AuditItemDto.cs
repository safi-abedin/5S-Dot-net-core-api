namespace api.DTOS.Audits
{
    public class AuditItemDto
    {
        public int? Id { get; set; }
        public int ChecklistItemId { get; set; }

        public int? ChecklistCatagoryId { get; set; }

        public string? CatagoryName { get; set; }
        public int? CatagoryOrder { get; set; }

        public string? CheckingItemName { get; set; }

        public string? EvaluationCriteria { get; set; }
        public int Order { get; set; }
        public int Score { get; set; }
    }
}
