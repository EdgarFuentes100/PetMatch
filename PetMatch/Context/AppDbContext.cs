using Microsoft.EntityFrameworkCore;
using PetMatch.Models;

namespace PetMatch.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }
        public DbSet<Publicacion> Publicaciones{ get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<TipoMascota> TiposMascota { get; set; }
        public DbSet<Solicitud> Solicitud { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Perfil> Perfil { get; set; }
        public DbSet<Mensaje> Mensajes { get; set; }
        public DbSet<PerfilMascotaIdeal> PerfilMascotaIdeal { get; set; }
        public DbSet<MascotaIdeal> MascotaIdeal { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Mensaje>()
                .HasOne(m => m.Emisor)
                .WithMany()
                .HasForeignKey(m => m.EmisorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Mensaje>()
                .HasOne(m => m.Receptor)
                .WithMany()
                .HasForeignKey(m => m.ReceptorId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
