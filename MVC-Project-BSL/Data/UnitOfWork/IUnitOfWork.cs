using MVC_Project_BSL.Data.Repository;
using MVC_Project_BSL.Models;
using static System.Net.Mime.MediaTypeNames;

namespace MVC_Project_BSL.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IGenericRepository<Groepsreis> GroepsreisRepository { get; }
        IGenericRepository<Bestemming> BestemmingRepository { get; }

        public void SaveChanges();
    }
}
