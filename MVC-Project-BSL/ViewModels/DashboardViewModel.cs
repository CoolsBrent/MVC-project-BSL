using MVC_Project_BSL.Models;
using static System.Net.Mime.MediaTypeNames;

namespace MVC_Project_BSL.ViewModels
{
    public class DashboardViewModel : CustomUser
    {
        public List<Groepsreis> Groepsreizen { get; set; }
        
    }
}
