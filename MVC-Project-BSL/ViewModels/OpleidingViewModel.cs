using MVC_Project_BSL.Models;
using System.Collections.Generic;

namespace MVC_Project_BSL.ViewModels
{
    public class OpleidingViewModel
    {
        public Opleiding Opleiding { get; set; }
        public IEnumerable<OpleidingPersoon> OpleidingPersonen { get; set; }
    }
}
