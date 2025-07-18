using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PetMatch.Controllers
{
    public class CitaController : Controller
    {
        // GET: CitaController
        public async Task<IActionResult> VistaCita()
        {
            return View();
        }
    }
}
