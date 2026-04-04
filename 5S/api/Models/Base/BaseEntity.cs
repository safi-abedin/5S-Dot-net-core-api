namespace api.Models.Base
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedAt { get; set;} = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }

    }
}
