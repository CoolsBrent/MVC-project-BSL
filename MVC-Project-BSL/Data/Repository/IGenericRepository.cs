using System.Linq.Expressions;

namespace MVC_Project_BSL.Data.Repository
{
    /// <summary>
    /// Interface die generieke repositoryfunctionaliteit definieert voor CRUD-operaties en queryondersteuning.
    /// </summary>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        #region Query Methods

        /// <summary>
        /// Retourneert een queryable object, optioneel met een queryfunctie voor verdere filtering.
        /// </summary>
        IQueryable<TEntity> GetQueryable(Func<IQueryable<TEntity>, IQueryable<TEntity>>? queryFunc = null);

        /// <summary>
        /// Haalt alle entiteiten asynchroon op, met optionele includes voor gerelateerde entiteiten.
        /// </summary>
        Task<IEnumerable<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null);

        /// <summary>
        /// Haalt een entiteit op basis van een integer-ID met specifieke includes voor gerelateerde entiteiten.
        /// </summary>
        Task<TEntity?> GetByIdWithIncludesAsync(int id, params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Haalt een entiteit op basis van een integer-ID.
        /// </summary>
        Task<TEntity?> GetByIdAsync(int id);

        /// <summary>
        /// Haalt een entiteit op basis van een string-ID.
        /// </summary>
        Task<TEntity?> GetByStringIdAsync(string id);

        /// <summary>
        /// Controleert of er een entiteit bestaat die voldoet aan de opgegeven voorwaarde.
        /// </summary>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Haalt de eerste entiteit op die voldoet aan de opgegeven voorwaarde.
        /// </summary>
        Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        #endregion

        #region CRUD Methods

        /// <summary>
        /// Voegt een nieuwe entiteit toe aan de database.
        /// </summary>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Werk een bestaande entiteit bij in de database.
        /// </summary>
        void Update(TEntity entity);

        /// <summary>
        /// Verwijdert een entiteit uit de database.
        /// </summary>
        void Delete(TEntity entity);

        /// <summary>
        /// Slaat alle wijzigingen in de database op.
        /// </summary>
        void Save();

        #endregion
    }
}