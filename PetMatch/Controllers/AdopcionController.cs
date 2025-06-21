using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetMatch.Context;      // tu namespace real
using PetMatch.Models;

namespace PetMatch.Controllers
{
    public class AdopcionController : Controller
    {
        private readonly AppDbContext _context;

        public AdopcionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Adopcion/VistaAdopcion
        [AllowAnonymous] // ✅ ESTA vista sí es pública
        public async Task<IActionResult> VistaAdopcion()
        {
            // 1) Las publicaciones que quieres mostrar
            var publicaciones = await _context.Publicaciones
                .Where(pub => !_context.Adopcion.Any(ado => ado.MascotaId == pub.MascotaId))
                .Include(p => p.Mascota)
                .ThenInclude(m => m.TipoMascota)
                .Include(p => p.Publicador)
                .ToListAsync();

            return View(publicaciones);
        }

        [AllowAnonymous]
        public async Task<IActionResult> VistaPublicacion() 
        {
            return View();
        }

    }
}
