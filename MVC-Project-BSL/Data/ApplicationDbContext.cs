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
        public DbSet<Groepsreis> Groepsreizen { get; set; }
        public DbSet<Bestemming> Bestemmingen { get; set; }
        public DbSet<Foto> Fotos { get; set; }
        public DbSet<Onkosten> Onkosten { get; set; }
        public DbSet<Kind> Kinderen { get; set; }
        public DbSet<Models.Monitor> Monitoren { get; set; }
        public DbSet<Opleiding> Opleidingen { get; set; }
        public DbSet<OpleidingPersoon> OpleidingPersonen { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine); // Log alle database-acties naar de console
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Groepsreis>()
                .HasMany(g => g.Activiteiten)
                .WithMany(a => a.Groepsreizen);

            modelBuilder.Entity<Groepsreis>()
                .HasMany(g => g.Kinderen)
                .WithMany(k => k.Groepsreizen);

            modelBuilder.Entity<Models.Monitor>()
                .HasMany(m => m.Groepsreizen)
                .WithMany(g => g.Monitoren);

            modelBuilder.Entity<Foto>()
                .HasOne(f => f.Bestemming)
                .WithMany(b => b.Fotos)
                .HasForeignKey(f => f.BestemmingId);

            // Relaties tussen Kind en CustomUser
            modelBuilder.Entity<Kind>()
                .HasOne(k => k.Persoon)
                .WithMany(u => u.Kinderen)
                .HasForeignKey(k => k.PersoonId);

            // Relaties tussen Monitor en CustomUser
            modelBuilder.Entity<Models.Monitor>()
                .HasOne(m => m.Persoon)
                .WithMany(u => u.Monitoren)
                .HasForeignKey(m => m.PersoonId);

            // Relaties tussen OpleidingPersoon en CustomUser
            modelBuilder.Entity<OpleidingPersoon>()
                .HasOne(o => o.Persoon)
                .WithMany(u => u.Opleidingen)
                .HasForeignKey(o => o.PersoonId);
        }
    }
}
