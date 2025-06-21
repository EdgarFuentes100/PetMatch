using System.ComponentModel.DataAnnotations;

namespace PetMatch.Models
{
    public class Rol
    {
        public int RolId { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; } = default!;

        // Navegación
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
