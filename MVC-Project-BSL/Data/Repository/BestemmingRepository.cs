using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data.Repository
{
    public class BestemmingRepository : GenericRepository<Bestemming>
    {
        public BestemmingRepository(ApplicationDbContext context) : base(context)
        {

        }

    }
}
