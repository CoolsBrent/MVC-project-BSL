using MVC_Project_BSL.Data.Repository;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IGenericRepository<Groepsreis> GroepsreisRepository { get; }
        IGenericRepository<Bestemming> BestemmingRepository { get; }
        IGenericRepository<Activiteit> ActiviteitRepository { get; }
        IGenericRepository<Models.Monitor> MonitorRepository { get; }
        IGenericRepository<Kind> KindRepository { get; }

        public void SaveChanges();
    }
}
