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
        }

        public IGenericRepository<Groepsreis> GroepsreisRepository { get; }
        public IGenericRepository<Bestemming> BestemmingRepository { get; private set; }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
