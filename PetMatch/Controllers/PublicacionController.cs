using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetMatch.Context;
using PetMatch.Models;
using System.Security.Claims;

namespace PetMatch.Controllers
{
    public class PublicacionController : Controller
    {
        private readonly AppDbContext _context;
        public PublicacionController(AppDbContext appDbContext) 
        {
            _context = appDbContext;
        }
        // GET: PublicacionController
        public async Task<IActionResult> MisPublicaciones()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var publicaciones = await _context.Publicaciones
                .Include(p => p.Mascota)
                .ThenInclude(m => m.TipoMascota)
                .Include(p => p.Publicador)
                .Where(p => p.Publicador.Email == email)
                .ToListAsync();

            var mascotasIds = publicaciones.Select(p => p.MascotaId).ToList();

            var solicitudes = await _context.Solicitud
                .Include(s => s.Solicitante)
                .Where(s => mascotasIds.Contains(s.MascotaId))
                .ToListAsync();

            ViewBag.Solicitudes = solicitudes;

            return View(publicaciones);
        }

        public async Task<IActionResult> HacerPublicacion()
        {
            return View();
        }
    }
}
