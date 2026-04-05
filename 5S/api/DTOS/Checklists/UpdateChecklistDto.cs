namespace api.DTOS.Checklists
{
    public class UpdateChecklistDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CheckingItemName { get; set; }
        public string EvaluationCriteria { get; set; }
        public int MaxScore { get; set; } = 5;
        public int Order { get; set; }
    }
}
