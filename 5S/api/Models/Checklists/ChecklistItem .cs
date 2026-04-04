using api.Models.Base;

namespace api.Models.Checklists
{
    public class ChecklistItem : BaseEntity
    {
        public int CategoryId { get; set; }
        public ChecklistCategory Category { get; set; }

        public string QuestionText { get; set; }
        public int MaxScore { get; set; } = 5;
        public int Order { get; set; }
    }
}
