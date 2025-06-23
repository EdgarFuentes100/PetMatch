using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PetMatch.Controllers
{
    public class PublicacionController : Controller
    {
        // GET: PublicacionController
        public async Task<IActionResult> MisPublicaciones()
        {
            return View();
        }

        public async Task<IActionResult> HacerPublicacion()
        {
            return View();
        }
    }
}
