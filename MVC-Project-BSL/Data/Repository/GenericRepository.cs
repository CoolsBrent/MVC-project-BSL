using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MVC_Project_BSL.Data.Repository
{
    /// <summary>
    /// Een generieke repositoryklasse voor het uitvoeren van CRUD-bewerkingen en complexe queries op entiteiten.
    /// Ondersteunt methoden voor ophalen, toevoegen, bijwerken, en verwijderen van gegevens, evenals query-functies met includes.
    /// </summary>
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        #region Private Fields
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        #endregion

        #region Constructor
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }
        #endregion

        #region Query Methods

        // Retourneert een queryable object, optioneel met meegegeven queryfuncties
        public IQueryable<TEntity> GetQueryable(
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryFunc = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (queryFunc != null)
            {
                query = queryFunc(query);
            }

            return query;
        }

        // Haalt alle entiteiten op met optionele include-functies
        public async Task<IEnumerable<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (include != null)
            {
                query = include(query);
            }

            return await query.ToListAsync();
        }

        // Haalt een entiteit op basis van een integer-ID
        public async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Haalt een entiteit op basis van een string-ID
        public async Task<TEntity?> GetByStringIdAsync(string id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Haalt een entiteit op met specifieke includes en een integer-ID
        public async Task<TEntity?> GetByIdWithIncludesAsync(int id, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _dbSet;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }
        #endregion

        #region CRUD Operations

        // Voegt een nieuwe entiteit asynchroon toe aan de database
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

        // Update een bestaande entiteit in de database
        public void Update(TEntity entity)
        {
            _dbSet.Update(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        // Verwijdert een entiteit uit de database
        public void Delete(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        #endregion

        #region Save Changes

        // Slaat alle wijzigingen op in de database
        public void Save()
        {
            _context.SaveChanges();
        }
        #endregion

        #region Helper Methods

        // Controleert of een bepaalde entiteit voldoet aan een opgegeven voorwaarde
        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

		// Haalt de eerste entiteit op die voldoet aan een opgegeven voorwaarde
		public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return await _dbSet.FirstOrDefaultAsync(predicate);
		}


		#endregion
	}
}