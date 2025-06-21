using System.ComponentModel.DataAnnotations;

namespace PetMatch.Models
{
    public class Perfil
    {
        // PK y FK (uno‑a‑uno con Usuario)
        [Key]
        public int UsuarioId { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }
        public string? Descripcion { get; set; }

        public Usuario Usuario { get; set; } = default!;
    }
}
