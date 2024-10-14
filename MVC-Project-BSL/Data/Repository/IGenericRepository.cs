using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MVC_Project_BSL.Data.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        //Om te controleren of er reeds bestaande foto's zijn in de EDIT
        IQueryable<TEntity> GetQueryable(
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryFunc = null);

        // Nieuwe GetAllAsync-methode met ondersteuning voor Include
        Task<IEnumerable<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null);

        // Nieuwe methode om een entiteit op te halen met inclusies
        Task<TEntity?> GetByIdWithIncludesAsync(int id, params Expression<Func<TEntity, object>>[] includes);
		
		Task<TEntity?> GetByIdAsync(int id);
		Task<TEntity?> GetByStringIdAsync(string id);
		Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        void Save();
    }
}
