using api.Models.Base;

namespace api.Repositories.Interfaces.Base
{
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> Query();

        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        Task SaveAsync();
    }
}
