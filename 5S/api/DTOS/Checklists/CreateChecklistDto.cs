namespace api.DTOS.Checklists
{
    public class CreateChecklistDto
    {
        public int CategoryId { get; set; }
        public string CheckingItemName { get; set; }
        public string EvaluationCriteria { get; set; }
        public int MaxScore { get; set; } = 5;
        public int Order { get; set; }
    }
}
