namespace api.DTOS.Checklists
{
    public class ChecklistResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CheckingItemName { get; set; }
        public string EvaluationCriteria { get; set; }
        public int MaxScore { get; set; }
        public int Order { get; set; }
    }
}
