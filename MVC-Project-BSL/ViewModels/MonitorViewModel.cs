namespace MVC_Project_BSL.ViewModels
{
    namespace MVC_Project_BSL.ViewModels
    {
        public class MonitorViewModel
        {
            public string PersoonId { get; set; } // Voeg PersoonId toe als je dat nodig hebt
            public string Voornaam { get; set; }   // Voornaam van de monitor
            public string Naam { get; set; }        // Achternaam van de monitor
            public bool IsHoofdMonitor { get; set; } // Is het een hoofdmonitor?
        }
    }

}
