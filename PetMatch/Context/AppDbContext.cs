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

    }
}
