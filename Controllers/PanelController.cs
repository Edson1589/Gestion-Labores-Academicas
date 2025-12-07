using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionLaboresAcademicas.Controllers
{
    [Authorize]
    public class PanelController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Director"))
                return RedirectToAction(nameof(Director));

            if (User.IsInRole("Secretaria"))
                return RedirectToAction(nameof(Secretaria));

            if (User.IsInRole("Docente"))
                return RedirectToAction(nameof(Docente));

            if (User.IsInRole("Regente"))
                return RedirectToAction(nameof(Regente));

            if (User.IsInRole("Padre"))
                return RedirectToAction(nameof(Padre));

            if (User.IsInRole("Estudiante"))
                return RedirectToAction(nameof(Estudiante));

            return View("Generico");
        }

        public IActionResult Director() => View();
        public IActionResult Secretaria() => View();
        public IActionResult Docente() => View();
        public IActionResult Estudiante() => View();
        public IActionResult Padre() => View();
        public IActionResult Regente() => View();
        public IActionResult Generico() => View();
    }
}
