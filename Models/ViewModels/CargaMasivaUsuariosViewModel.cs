using System.ComponentModel.DataAnnotations;

namespace GestionLaboresAcademicas.Models.ViewModels
{
    public class CargaMasivaUsuariosViewModel
    {
        [Required]
        [Display(Name = "Archivo CSV de usuarios")]
        public IFormFile? Archivo { get; set; }
        public List<ResultadoFilaCargaMasiva> Resultados { get; set; } = new();

        public int TotalRegistros { get; set; }
        public int RegistrosValidos { get; set; }
        public int RegistrosInvalidos { get; set; }

        public string? DatosValidosJson { get; set; }

        public int? UsuariosCreados { get; set; }
        public int? UsuariosFallidos { get; set; }

        public bool PuedeConfirmar => RegistrosValidos > 0 && UsuariosCreados == null;
    }

    public class ResultadoFilaCargaMasiva
    {
        public int NumeroLinea { get; set; }

        public string TipoUsuario { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string DocumentoCI { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;

        public bool EsValido { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}
