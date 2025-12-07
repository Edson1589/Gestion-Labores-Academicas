namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class TotalIndicadorViewModel
    {
        public string Nombre { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public decimal Valor { get; set; }
        public string Unidad { get; set; } = string.Empty;
    }
}
