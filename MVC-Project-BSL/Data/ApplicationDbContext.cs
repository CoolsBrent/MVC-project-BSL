using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data
{
    /// <summary>
    /// ApplicationDbContext representeert de databasecontext voor de applicatie en beheert
    /// de entiteiten, hun relaties en het Identity-systeem voor gebruikersbeheer.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<CustomUser, IdentityRole<int>, int>
    {
        #region Constructor
        /// <summary>
        /// Initialisatie van de ApplicationDbContext met configuratieopties.
        /// </summary>
        /// <param name="options">Opties voor de configuratie van de databasecontext.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        #endregion

        #region DbSets
        /// <summary> Activiteiten in de applicatie. </summary>
        public DbSet<Activiteit> Activiteiten { get; set; }

        /// <summary> Onkosten records gelinkt aan groepsreizen. </summary>
        public DbSet<Onkosten> Onkosten { get; set; }

        /// <summary> Opleidingen die gebruikers kunnen volgen. </summary>
        public DbSet<Opleiding> Opleidingen { get; set; }

        /// <summary> Koppeltabellen voor gebruikers en opleidingen. </summary>
        public DbSet<OpleidingPersoon> OpleidingPersonen { get; set; }

        /// <summary> Programma's die activiteiten en groepsreizen verbinden. </summary>
        public DbSet<Programma> Programmas { get; set; }

        /// <summary> Bestemmingen voor groepsreizen. </summary>
        public DbSet<Bestemming> Bestemmingen { get; set; }

        /// <summary> Groepsreizen in de applicatie. </summary>
        public DbSet<Groepsreis> Groepsreizen { get; set; }

        /// <summary> Monitoren die groepsreizen begeleiden. </summary>
        public DbSet<Models.Monitor> Monitoren { get; set; }

        /// <summary> Kinderen die deelnemen aan groepsreizen. </summary>
        public DbSet<Kind> Kinderen { get; set; }

        /// <summary> Foto's gekoppeld aan bestemmingen. </summary>
        public DbSet<Foto> Fotos { get; set; }

        /// <summary> Deelnemers aan groepsreizen. </summary>
        public DbSet<Deelnemer> Deelnemers { get; set; }

        /// <summary> Custom gebruikers in de applicatie. </summary>
        public DbSet<CustomUser> CustomUsers { get; set; }
        #endregion

        #region OnModelCreating
        /// <summary>
        /// Configuratie van de database-entiteiten, sleutels en relaties tussen de modellen.
        /// </summary>
        /// <param name="modelBuilder">De ModelBuilder om de modellen in te configureren.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Auto-increment ID for CustomUser
            modelBuilder.Entity<CustomUser>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            // R1 + R2: Activiteit - Groepsreis (many-to-many via Programma)
            modelBuilder.Entity<Programma>()
                .HasKey(p => new { p.ActiviteitId, p.GroepsreisId });
            modelBuilder.Entity<Programma>()
                .HasOne(p => p.Activiteit)
                .WithMany(a => a.Programmas)
                .HasForeignKey(p => p.ActiviteitId);
            modelBuilder.Entity<Programma>()
                .HasOne(p => p.Groepsreis)
                .WithMany(g => g.Programmas)
                .HasForeignKey(p => p.GroepsreisId);

            // R3: Bestemming - Groepsreis (one-to-many)
            modelBuilder.Entity<Groepsreis>()
                .HasOne(g => g.Bestemming)
                .WithMany(b => b.Groepsreizen)
                .HasForeignKey(g => g.BestemmingId);

            // R4: Bestemming - Foto (one-to-many)
            modelBuilder.Entity<Foto>()
                .HasOne(f => f.Bestemming)
                .WithMany(b => b.Fotos)
                .HasForeignKey(f => f.BestemmingId);

            // R5: Groepsreis - Onkosten (one-to-many)
            modelBuilder.Entity<Onkosten>()
                .HasOne(o => o.Groepsreis)
                .WithMany(g => g.Onkosten)
                .HasForeignKey(o => o.GroepsreisId);

            // R6 + R8: Groepsreis - Kind (many-to-many via Deelnemer)
            modelBuilder.Entity<Deelnemer>()
                .HasOne(d => d.Kind)
                .WithMany()
                .HasForeignKey(d => d.KindId);
            modelBuilder.Entity<Deelnemer>()
                .HasOne(d => d.GroepsreisDetail)
                .WithMany(g => g.Deelnemers)
                .HasForeignKey(d => d.GroepsreisDetailId);

            // R7: Groepsreis - Monitor (many-to-many via GroepsreisMonitor)
            modelBuilder.Entity<GroepsreisMonitor>()
                .HasKey(gm => new { gm.GroepsreisId, gm.MonitorId });
            modelBuilder.Entity<GroepsreisMonitor>()
                .HasOne(gm => gm.Groepsreis)
                .WithMany(g => g.Monitoren)
                .HasForeignKey(gm => gm.GroepsreisId);
            modelBuilder.Entity<GroepsreisMonitor>()
                .HasOne(gm => gm.Monitor)
                .WithMany(m => m.Groepsreizen)
                .HasForeignKey(gm => gm.MonitorId);

            // R9: Opleiding - Opleiding (self-referencing)
            modelBuilder.Entity<Opleiding>()
                .HasOne(o => o.OpleidingVereist)
                .WithMany(o => o.OpleidingenAfhankelijk)
                .HasForeignKey(o => o.OpleidingVereistId)
                .OnDelete(DeleteBehavior.Restrict);

            // R11: CustomUser - Kind (one-to-many)
            modelBuilder.Entity<Kind>()
                .HasOne(k => k.Persoon)
                .WithMany(u => u.Kinderen)
                .HasForeignKey(k => k.PersoonId)
                .OnDelete(DeleteBehavior.Cascade);

            // R12 + R13: CustomUser - Opleiding (many-to-many via OpleidingPersoon)
            modelBuilder.Entity<OpleidingPersoon>()
                .HasKey(op => op.Id);
			modelBuilder.Entity<OpleidingPersoon>()
                .HasOne(op => op.Opleiding)
                .WithMany(o => o.OpleidingPersonen)
                .HasForeignKey(op => op.OpleidingId);
            modelBuilder.Entity<OpleidingPersoon>()
                .HasOne(op => op.Persoon)
                .WithMany(u => u.Opleidingen)
                .HasForeignKey(op => op.PersoonId);
        }
        #endregion
    }
}