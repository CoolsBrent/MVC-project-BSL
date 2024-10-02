using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.ViewModels
{
    public class RoleManagementViewModel
    {
        // Lijst van alle rollen
        public List<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();

        // Lijst van alle gebruikers
        public List<UserRolesViewModel> Users { get; set; } = new List<UserRolesViewModel>();

        // Een geselecteerde gebruiker
        public string SelectedUserId { get; set; }

        // De rol die je wilt toewijzen aan de geselecteerde gebruiker
        public string SelectedRole { get; set; }

        // De rollen van de geselecteerde gebruiker
        public IList<string> UserRoles { get; set; } = new List<string>();
    }
}

