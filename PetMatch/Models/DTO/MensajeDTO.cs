namespace PetMatch.Models.DTO
{
    public class MensajeDTO
    {
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool EsPropio { get; set; }
    }
}
