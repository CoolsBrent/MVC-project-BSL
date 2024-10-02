using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        // Nieuwe GetAllAsync-methode met ondersteuning voor Include
        Task<IEnumerable<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null);

        Task<TEntity?> GetByIdAsync(int id);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        void Save();
    }
}
