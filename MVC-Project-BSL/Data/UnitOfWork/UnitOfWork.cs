using MVC_Project_BSL.Data.Repository;
using MVC_Project_BSL.Models;
using static System.Net.Mime.MediaTypeNames;

namespace MVC_Project_BSL.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            GroepsreisRepository = new GenericRepository<Groepsreis>(_context);
            BestemmingRepository = new GenericRepository<Bestemming>(_context);
            ActiviteitRepository = new GenericRepository<Activiteit>(_context);
            MonitorRepository = new GenericRepository<Models.Monitor>(_context);
            KindRepository = new GenericRepository<Kind>(_context);
            FotoRepository = new GenericRepository<Foto>(_context);
        }

        public IGenericRepository<Groepsreis> GroepsreisRepository { get; }
        public IGenericRepository<Bestemming> BestemmingRepository { get; private set; }
        public IGenericRepository<Activiteit> ActiviteitRepository { get; private set; }
        public IGenericRepository<Models.Monitor> MonitorRepository { get; private set; }
        public IGenericRepository<Kind> KindRepository { get; private set; }
        public IGenericRepository<Foto> FotoRepository { get; }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
