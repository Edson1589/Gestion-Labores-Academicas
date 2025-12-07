namespace GestionLaboresAcademicas.Models.DatosAcademicos
{
    public class DatoAcademico
    {
        public int Id { get; set; }

        public int PeriodoAcademicoId { get; set; }
        public PeriodoAcademico PeriodoAcademico { get; set; } = null!;

        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;

        public int AsignaturaId { get; set; }
        public Asignatura Asignatura { get; set; } = null!;

        public int EstudianteId { get; set; }
        public Usuario Estudiante { get; set; } = null!;
    }
}
