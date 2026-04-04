using api.Models.Base;

namespace api.Models.Checklists
{
    public class ChecklistCategory : BaseEntity
    {
        public string Name { get; set; }
        public int Order { get; set; }
    }
}
