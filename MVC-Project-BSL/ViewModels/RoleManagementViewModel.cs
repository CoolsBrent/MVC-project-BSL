using Microsoft.AspNetCore.Identity;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.ViewModels
{
    public class RoleManagementViewModel
    {
        // Lijst van alle rollen
        public List<IdentityRole<int>> Roles { get; set; } = new List<IdentityRole<int>>();

        // Lijst van alle gebruikers
        public List<UserRolesViewModel> Users { get; set; } = new List<UserRolesViewModel>();

        // Een geselecteerde gebruiker
        public int SelectedUserId { get; set; }

		public int SelectedChildId { get; set; }

		// De rol die je wilt toewijzen aan de geselecteerde gebruiker
		public string SelectedRole { get; set; }

        // De rollen van de geselecteerde gebruiker
        public IList<string> UserRoles { get; set; } = new List<string>();
    }

    public class UserRolesViewModel
    {
        public CustomUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}

