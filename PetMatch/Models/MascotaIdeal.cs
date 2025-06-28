namespace PetMatch.Models
{
    public class MascotaIdeal
    {
        public int MascotaIdealId { get; set; }

        public int PerfilId { get; set; }
        public PerfilMascotaIdeal Perfil { get; set; }

        public int MascotaId { get; set; }
        public Mascota Mascota { get; set; }
    }
}
