using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.ViewModels
{
    public class UserRolesViewModel
    {
        public CustomUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}
