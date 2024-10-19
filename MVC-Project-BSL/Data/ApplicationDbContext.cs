using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVC_Project_BSL.Models;

namespace MVC_Project_BSL.Data
{
    public class ApplicationDbContext : IdentityDbContext<CustomUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Activiteit> Activiteiten { get; set; }
        public DbSet<Onkosten> Onkosten { get; set; }
        public DbSet<Opleiding> Opleidingen { get; set; }
        public DbSet<OpleidingPersoon> OpleidingPersonen { get; set; }
        public DbSet<Programma> Programmas { get; set; }
        public DbSet<Bestemming> Bestemmingen { get; set; }
        public DbSet<Groepsreis> Groepsreizen { get; set; }
        public DbSet<Monitor> Monitoren { get; set; }
        public DbSet<Kind> Kinderen { get; set; }
        public DbSet<Foto> Fotos { get; set; }
        public DbSet<Deelnemer> Deelnemers { get; set; }
        public DbSet<CustomUser> CustomUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                .HasOne<Opleiding>()
                .WithMany()
                .HasForeignKey(o => o.OpleidingVereist);

            // R11: CustomUser - Kind (one-to-many)
            modelBuilder.Entity<Kind>()
                .HasOne(k => k.Persoon)
                .WithMany(u => u.Kinderen)
                .HasForeignKey(k => k.PersoonId);

            // R12 + R13: CustomUser - Opleiding (many-to-many via OpleidingPersoon)
            modelBuilder.Entity<OpleidingPersoon>()
                .HasKey(op => new { op.OpleidingId, op.PersoonId });
            modelBuilder.Entity<OpleidingPersoon>()
                .HasOne(op => op.Opleiding)
                .WithMany(o => o.OpleidingPersonen)
                .HasForeignKey(op => op.OpleidingId);
            modelBuilder.Entity<OpleidingPersoon>()
                .HasOne(op => op.Persoon)
                .WithMany(u => u.Opleidingen)
                .HasForeignKey(op => op.PersoonId);
        }
    }
}
