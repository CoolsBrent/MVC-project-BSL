using MVC_Project_BSL.Data.Repository;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data.UnitOfWork
{
    /// <summary>
    /// Implementeert het eenheid-van-werk-patroon voor het beheren van repositories en 
    /// het opslaan van wijzigingen in de databasecontext.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        #region Private Fields
        private readonly ApplicationDbContext _context;
        #endregion

        #region Constructor

        /// <summary>
        /// Initialiseert een nieuwe instantie van de <see cref="UnitOfWork"/> klasse met de gegeven databasecontext.
        /// </summary>
        /// <param name="context">De databasecontext om mee te werken.</param>
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            CustomUserRepository = new GenericRepository<CustomUser>(_context);
            DeelnemerRepository = new GenericRepository<Deelnemer>(_context);
            GroepsreisRepository = new GenericRepository<Groepsreis>(_context);
            BestemmingRepository = new GenericRepository<Bestemming>(_context);
            ActiviteitRepository = new GenericRepository<Activiteit>(_context);
            MonitorRepository = new GenericRepository<Models.Monitor>(_context);
            KindRepository = new GenericRepository<Kind>(_context);
            ProgrammaRepository = new GenericRepository<Programma>(_context);
            FotoRepository = new GenericRepository<Foto>(_context);
            OnkostenRepository = new GenericRepository<Onkosten>(_context);
            OpleidingRepository = new GenericRepository<Opleiding>(_context);
        }


        #endregion

        #region Repositories

        /// <summary>
        /// Repository voor het beheren van groepsreizen.
        /// </summary>
        public IGenericRepository<Groepsreis> GroepsreisRepository { get; }

        /// <summary>
        /// Repository voor het beheren van deelnemers.
        /// </summary>
        public IGenericRepository<Deelnemer> DeelnemerRepository { get; private set; }

        /// <summary>
        /// Repository voor het beheren van gebruikers.
        /// </summary>
        public IGenericRepository<CustomUser> CustomUserRepository { get; private set; }

        /// <summary>
        /// Repository voor het beheren van bestemmingen.
        /// </summary>
        public IGenericRepository<Bestemming> BestemmingRepository { get; private set; }

        /// <summary>
        /// Repository voor het beheren van activiteiten.
        /// </summary>
        public IGenericRepository<Activiteit> ActiviteitRepository { get; private set; }

        /// <summary>
        /// Repository voor het beheren van monitoren.
        /// </summary>
        public IGenericRepository<Models.Monitor> MonitorRepository { get; private set; }

        /// <summary>
        /// Repository voor het beheren van kinderen.
        /// </summary>
        public IGenericRepository<Kind> KindRepository { get; private set; }

        /// <summary>
        /// Repository voor het beheren van programma's.
        /// </summary>
        public IGenericRepository<Programma> ProgrammaRepository { get; private set; }

        /// <summary>
        /// Repository voor het beheren van foto's.
        /// </summary>
        public IGenericRepository<Foto> FotoRepository { get; }
		/// <summary>
		/// Repository voor het beheren van opleidingen.
		/// </summary>
		public IGenericRepository<Opleiding> OpleidingRepository { get; private set; }

		/// <summary>
		/// Repository voor het beheren van onkosten.
		/// </summary>
		public IGenericRepository<Onkosten> OnkostenRepository { get; private set; }

		#endregion

		#region Save Changes

		/// <summary>
		/// Slaat alle wijzigingen op in de database.
		/// </summary>
		public void SaveChanges()
        {
            _context.SaveChanges();
        }

        #endregion
    }
}