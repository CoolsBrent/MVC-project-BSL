using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data.Repository
{
    public class FotoRepository : GenericRepository<Foto>
    {
        public FotoRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}
