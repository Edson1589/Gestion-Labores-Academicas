using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionLaboresAcademicas.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [Authorize(Roles = "Director")]
        public IActionResult Director()
        {
            return View();
        }

        [Authorize(Roles = "Secretaria")]
        public IActionResult Secretaria()
        {
            return View();
        }

        [Authorize(Roles = "Docente")]
        public IActionResult Docente()
        {
            return View();
        }

        [Authorize(Roles = "Estudiante")]
        public IActionResult Estudiante()
        {
            return View();
        }

        [Authorize(Roles = "Padre")]
        public IActionResult Padre()
        {
            return View();
        }

        [Authorize(Roles = "Regente")]
        public IActionResult Regente()
        {
            return View();
        }

        [Authorize(Roles = "Bibliotecario")]
        public IActionResult Bibliotecario()
        {
            return View();
        }
    }
}
