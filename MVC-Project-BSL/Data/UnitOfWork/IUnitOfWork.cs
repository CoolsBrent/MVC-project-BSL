using MVC_Project_BSL.Data.Repository;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data.UnitOfWork
{
	public interface IUnitOfWork
	{
		IGenericRepository<Groepsreis> GroepsreisRepository { get; }
		IGenericRepository<CustomUser> CustomUserRepository { get; }
		IGenericRepository<Bestemming> BestemmingRepository { get; }
		IGenericRepository<Activiteit> ActiviteitRepository { get; }
		IGenericRepository<Monitor> MonitorRepository { get; }
		IGenericRepository<Deelnemer> DeelnemerRepository { get; }
		IGenericRepository<Programma> ProgrammaRepository { get; }
		IGenericRepository<Kind> KindRepository { get; }
		IGenericRepository<Foto> FotoRepository { get; }

		public void SaveChanges();
	}
}