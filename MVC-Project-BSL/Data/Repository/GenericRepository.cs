using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MVC_Project_BSL.Data.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        // Nieuwe methode met ondersteuning voor include
        public async Task<IEnumerable<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
        {
            IQueryable<TEntity> query = _dbSet;

            // Pas includes toe als de functie wordt meegegeven
            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
		public async Task<TEntity?> GetByStringIdAsync(string id)
		{
			return await _dbSet.FindAsync(id);
		}

		// Nieuwe methode om een entiteit op te halen met inclusies
		public async Task<TEntity?> GetByIdWithIncludesAsync(int id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            // Pas de includes toe op de query
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }
	

		public async Task AddAsync(TEntity entity)
        {
            try
            {
                await _dbSet.AddAsync(entity);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
		public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return await _dbSet.AnyAsync(predicate);
		}
		public async Task<TEntity?> GetByPersoonIdAsync(string persoonId)
		{
			// Zorg ervoor dat TEntity een eigenschap heeft met de naam "PersoonId"
			return await _dbSet.FirstOrDefaultAsync(e => EF.Property<string>(e, "PersoonId") == persoonId);
		}

	}
}
