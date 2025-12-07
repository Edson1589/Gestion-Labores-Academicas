namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class EstudianteSeleccionViewModel
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = null!;
        public string DocumentoCI { get; set; } = null!;
        public string CursoNombre { get; set; } = string.Empty;
        public bool Seleccionado { get; set; }
    }
}
