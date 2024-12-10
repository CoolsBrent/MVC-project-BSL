using MVC_Project_BSL.Data.Repository;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data.UnitOfWork
{
    /// <summary>
    /// Interface voor het eenheid-van-werk-patroon dat repositories beheert voor de verschillende entiteiten en 
    /// wijzigingen in de context opslaat.
    /// </summary>
    public interface IUnitOfWork
    {
        #region Repositories

        /// <summary>
        /// Repository voor het beheren van groepsreizen.
        /// </summary>
        IGenericRepository<Groepsreis> GroepsreisRepository { get; }

        /// <summary>
        /// Repository voor het beheren van gebruikers.
        /// </summary>
        IGenericRepository<CustomUser> CustomUserRepository { get; }

        /// <summary>
        /// Repository voor het beheren van bestemmingen.
        /// </summary>
        IGenericRepository<Bestemming> BestemmingRepository { get; }

        /// <summary>
        /// Repository voor het beheren van activiteiten.
        /// </summary>
        IGenericRepository<Activiteit> ActiviteitRepository { get; }

        /// <summary>
        /// Repository voor het beheren van monitoren.
        /// </summary>
        IGenericRepository<Models.Monitor> MonitorRepository { get; }

        /// <summary>
        /// Repository voor het beheren van deelnemers.
        /// </summary>
        IGenericRepository<Deelnemer> DeelnemerRepository { get; }

        /// <summary>
        /// Repository voor het beheren van programma's.
        /// </summary>
        IGenericRepository<Programma> ProgrammaRepository { get; }

        /// <summary>
        /// Repository voor het beheren van kinderen.
        /// </summary>
        IGenericRepository<Kind> KindRepository { get; }

        /// <summary>
        /// Repository voor het beheren van foto's.
        /// </summary>
        IGenericRepository<Foto> FotoRepository { get; }
		/// <summary>
		/// Repository voor het beheren van opleidingen.
		/// </summary>
		IGenericRepository<Opleiding> OpleidingRepository { get; }


		/// <summary>
		/// Repository voor het beheren van onkosten.
		/// </summary>
		IGenericRepository<Onkosten> OnkostenRepository { get; }

		#endregion

		#region Save Changes

		/// <summary>
		/// Slaat alle wijzigingen in de context op naar de database.
		/// </summary>
		void SaveChanges();

        #endregion
    }
}