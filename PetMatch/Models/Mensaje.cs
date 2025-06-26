namespace PetMatch.Models
{
    public class Mensaje
    {
        public int Id { get; set; }
        // Guarda los ID de usuario
        public int EmisorId { get; set; }
        public int ReceptorId { get; set; }

        // Guarda los objetos completos de los usuarios
        public Usuario Emisor { get; set; } = default!;
        public Usuario Receptor { get; set; } = default!;

        public string Contenido { get; set; } = default!;
        public DateTime FechaEnvio { get; set; }

        public bool Leido { get; set; } = false;
    }
}
