namespace PetMatch.Models.DTO
{
    public class RecomendacionDTO
    {
        public string EtiquetaGenerada { get; set; }
        public List<int> MascotasRecomendadasIds { get; set; } = new List<int>();
    }
}
