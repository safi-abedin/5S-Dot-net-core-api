using api.Data;
using api.Models.Base;
using api.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories.Base
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _db;

        public Repository(AppDbContext context)
        {
            _context = context;
            _db = context.Set<T>();
        }

        public IQueryable<T> Query() => _db.AsQueryable();

        public async Task<List<T>> GetAllAsync()
            => await _db.ToListAsync();

        public async Task<T> GetByIdAsync(int id)
            => await _db.FindAsync(id);

        public async Task AddAsync(T entity)
            => await _db.AddAsync(entity);

        public void Update(T entity) => _db.Update(entity);

        public void Delete(T entity) => _db.Remove(entity);

        public async Task SaveAsync()
            => await _context.SaveChangesAsync();
    }
}
