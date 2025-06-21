using System.ComponentModel.DataAnnotations;

namespace PetMatch.Models
{
    public class Mascota
    {
        public int MascotaId { get; set; }

        [Required, StringLength(80)]
        public string Nombre { get; set; } = default!;

        public int? EdadMeses { get; set; }
        public char? Sexo { get; set; }              // 'M' / 'H'
        public string? Tamano { get; set; }          // Pequeño, Mediano...
        public string? FotoUrl { get; set; }
        public string Temperamento { get; set; }

        // FK
        public int TipoMascotaId { get; set; }
        public TipoMascota TipoMascota { get; set; } = default!;

        // Navegaciones
        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();
        public ICollection<Adopcion> Adopciones { get; set; } = new List<Adopcion>();
    }
}
