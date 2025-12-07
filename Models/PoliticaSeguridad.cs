namespace GestionLaboresAcademicas.Models
{
    public class PoliticaSeguridad
    {
        public int Id { get; set; }

        public int LongitudMinimaPassword { get; set; } = 8;
        public bool RequiereMayusculas { get; set; } = true;
        public bool RequiereMinusculas { get; set; } = true;
        public bool RequiereNumero { get; set; } = true;
        public bool RequiereCaracterEspecial { get; set; } = true;

        public int IntentosMaximosFallidos { get; set; } = 5;
        public int MinutosBloqueo { get; set; } = 15;
    }
}
