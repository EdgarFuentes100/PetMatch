namespace PetMatch.Models
{
    public class Solicitud
    {
        public int SolicitudId { get; set; }

        // FK a Mascota
        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; } = default!;

        // FK a Usuario (solicitante)
        public int SolicitanteId { get; set; }
        public Usuario Solicitante { get; set; } = default!;

        public char Estado { get; set; } = 'P';  // P = pendiente, A = aprobada, R = rechazada
    }
}
