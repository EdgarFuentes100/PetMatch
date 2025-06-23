using System.ComponentModel.DataAnnotations;

namespace PetMatch.Models
{
    public class Usuario
    {
        public int UsuarioId { get; set; }

        [Required, EmailAddress, StringLength(255)]
        public string Email { get; set; } = default!;

        [Required, StringLength(100)]
        public string Nombre { get; set; } = default!;

        // FK
        public int RolId { get; set; }
        public Rol Rol { get; set; } = default!;

        // Navegaciones
        public Perfil? Perfil { get; set; }
        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();
        public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
    }
}
