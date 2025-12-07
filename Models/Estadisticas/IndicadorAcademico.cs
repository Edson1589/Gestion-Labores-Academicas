namespace GestionLaboresAcademicas.Models.Estadisticas
{
    public class IndicadorAcademico
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public TipoIndicador Tipo { get; set; }
        public string? Descripcion { get; set; }

        public decimal Valor { get; set; }
        public string Unidad { get; set; } = "%";
        public decimal? Umbral { get; set; }
        public string? ClaveAgrupacion1 { get; set; }
        public string? ClaveAgrupacion2 { get; set; }
    }
}
