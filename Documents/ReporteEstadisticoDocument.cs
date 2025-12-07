using GestionLaboresAcademicas.Models.Estadisticas;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;

namespace GestionLaboresAcademicas.Documents
{
    public class ReporteEstadisticoDocument : IDocument
    {
        private readonly ReporteEstadistico _reporte;

        public ReporteEstadisticoDocument(ReporteEstadistico reporte)
        {
            _reporte = reporte;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(TextStyle.Default.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Página ");
                    txt.CurrentPageNumber();
                    txt.Span(" / ");
                    txt.TotalPages();
                });
            });
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Institución educativa").SemiBold().FontSize(14);
                        col.Item().Text(_reporte.NombreInstitucion ?? string.Empty);
                    });

                    row.ConstantItem(80).AlignRight().Text("LOGO");
                });

                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Reporte estadístico académico").SemiBold().FontSize(12);
                });

                column.Item().PaddingTop(2).Text(text =>
                {
                    text.Span("Usuario: ").SemiBold();
                    text.Span(_reporte.NombreUsuario ?? "");
                    text.Span(" (");
                    text.Span(_reporte.RolUsuario ?? "");
                    text.Span(")   ");

                    text.Span("Fecha/hora: ").SemiBold();
                    text.Span(_reporte.FechaGeneracion.ToLocalTime().ToString("g"));
                });

                if (_reporte.Filtros != null)
                {
                    var f = _reporte.Filtros;
                    column.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("Filtros: ").SemiBold();
                        text.Span($"Periodo={f.PeriodoAcademicoId?.ToString() ?? "-"}, ");
                        text.Span($"Curso={f.CursoId?.ToString() ?? "-"}, ");
                        text.Span($"Asignatura={f.AsignaturaId?.ToString() ?? "-"}");
                    });
                }
            });
        }

        void ComposeContent(IContainer container)
        {
            var indicadores = _reporte.Indicadores ?? new List<IndicadorAcademico>();

            if (!indicadores.Any())
            {
                container.Text("Sin datos para los filtros seleccionados.");
                return;
            }

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Grupo (Curso)");
                    header.Cell().Element(HeaderCell).Text("Detalle");
                    header.Cell().Element(HeaderCell).Text("Indicador");
                    header.Cell().Element(HeaderCell).Text("Tipo");
                    header.Cell().Element(HeaderCell).Text("Valor");
                    header.Cell().Element(HeaderCell).Text("Unidad");
                });

                foreach (var ind in indicadores)
                {
                    table.Cell().Element(BodyCell).Text(ind.ClaveAgrupacion1 ?? string.Empty);
                    table.Cell().Element(BodyCell).Text(ind.ClaveAgrupacion2 ?? string.Empty);
                    table.Cell().Element(BodyCell).Text(ind.Nombre);
                    table.Cell().Element(BodyCell).Text(ind.Tipo.ToString());
                    table.Cell().Element(BodyCell).Text(ind.Valor.ToString("0.##"));
                    table.Cell().Element(BodyCell).Text(ind.Unidad);
                }
            });
        }

        static IContainer HeaderCell(IContainer container) =>
            container
                .DefaultTextStyle(TextStyle.Default.SemiBold())
                .PaddingVertical(4)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Medium);

        static IContainer BodyCell(IContainer container) =>
            container
                .PaddingVertical(2)
                .BorderBottom(0.5f)
                .BorderColor(Colors.Grey.Lighten2);
    }
}
