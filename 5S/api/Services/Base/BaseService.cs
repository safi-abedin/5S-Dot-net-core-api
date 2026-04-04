using api.Models.Base;
using api.Repositories.Interfaces.Base;

namespace api.Services.Base
{
    public class BaseService<T> where T : BaseEntity
    {
        protected readonly IRepository<T> _repo;

        public BaseService(IRepository<T> repo)
        {
            _repo = repo;
        }
    }
}
