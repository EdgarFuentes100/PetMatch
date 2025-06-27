namespace PetMatch.Models.DTO
{
    public class MensajeResumenDTO
    {
        public int EmisorId { get; set; }
        public int ReceptorId { get; set; }
        public string Contenido { get; set; }
        public bool Leido { get; set; }
        public int OtroUsuarioId { get; set; }
        public string OtroNombre { get; set; }
    }
}
