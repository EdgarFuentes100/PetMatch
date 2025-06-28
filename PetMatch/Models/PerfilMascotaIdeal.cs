namespace PetMatch.Models
{
    public class PerfilMascotaIdeal
    {
        public int PerfilMascotaIdealId { get; set; }

        public int UsuarioId { get; set; } // ID del usuario (si usas Identity)
        public string TextoEstiloVida { get; set; } // Lo que el usuario escribió
        public string EtiquetaGenerada { get; set; } // Ej: "Activo", "Casero"
        public string TiposMascotaIdeales { get; set; } // Ej: "Perro activo, Gato tranquilo"
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public ICollection<MascotaIdeal> MascotasIdeales { get; set; }
    }
}
