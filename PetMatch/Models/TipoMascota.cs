using System.ComponentModel.DataAnnotations;

namespace PetMatch.Models
{
    public class TipoMascota
    {
        public int TipoMascotaId { get; set; }

        [Required, StringLength(60)]
        public string Nombre { get; set; } = default!;


        // Navegación
        public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    }
}
