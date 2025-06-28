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
        public int NivelEnergia { get; set; } // Escala 1-5
        public bool Sociable { get; set; }
        public bool RequiereEjercicio { get; set; }
        public string Raza { get; set; }
        public string Descripcion { get; set; }

        public bool CompatibleConNiños { get; set; }
        public bool CompatibleConOtrasMascotas { get; set; }
        public int NivelCuidado { get; set; }
        public string AmbienteIdeal { get; set; }

        // FK
        public int TipoMascotaId { get; set; }
        public TipoMascota TipoMascota { get; set; } = default!;

        // Navegaciones
        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();
        public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
        public ICollection<MascotaIdeal> MascotasIdeales { get; set; }

    }
}
