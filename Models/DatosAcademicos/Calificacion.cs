namespace GestionLaboresAcademicas.Models.DatosAcademicos
{
    public class Calificacion : DatoAcademico
    {
        public decimal Nota { get; set; }

        public string TipoEvaluacion { get; set; } = null!;

        public DateTime Fecha { get; set; }
    }
}
