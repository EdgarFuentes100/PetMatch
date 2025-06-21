using System.ComponentModel.DataAnnotations;

namespace PetMatch.Models
{
    public class Publicacion
    {
        public int PublicacionId { get; set; }

        // FK a Mascota
        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; } = default!;

        // FK a Usuario (publicador)
        public int PublicadorId { get; set; }
        public Usuario Publicador { get; set; } = default!;

        [StringLength(120)]
        public string? Titulo { get; set; }

        public string? Historia { get; set; }
        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    }
}
