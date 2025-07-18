using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetMatch.Context;
using PetMatch.Models;
using PetMatch.Services;
using System.Linq;
using System.Threading.Tasks;

namespace PetMatch.Controllers
{
    public class SolicitudController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ServiceTransactionHelper _transactionHelper;

        public SolicitudController(AppDbContext context, ServiceTransactionHelper serviceTransaction)
        {
            _context = context;
            _transactionHelper = serviceTransaction;
        }

        [AllowAnonymous]
        public async Task<IActionResult> VistaPublicaciones()
        {
            var publicaciones = await _context.Publicaciones
                .Include(p => p.Mascota)
                .Include(p => p.Publicador)
                .ToListAsync();

            var mascotasIds = publicaciones.Select(p => p.MascotaId).ToList();

            var solicitudes = await _context.Solicitud
                .Include(s => s.Solicitante)
                .Where(s => mascotasIds.Contains(s.MascotaId))
                .ToListAsync();

            ViewBag.Solicitudes = solicitudes;

            return View(publicaciones);
        }

        [HttpPost]
        public async Task<IActionResult> AceptarSolicitud(int solicitudId)
        {
            var solicitud = await _context.Solicitud.FindAsync(solicitudId);
            if (solicitud != null)
            {
                solicitud.Estado = 'A';
                await _context.SaveChangesAsync();
                return Json(new { success = true, mascotaId = solicitud.MascotaId });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerConteos(int mascotaId)
        {
            var solicitudes = await _context.Solicitud
                .Where(s => s.MascotaId == mascotaId)
                .ToListAsync();

            var conteos = new
            {
                aceptadas = solicitudes.Count(s => s.Estado == 'A')
            };

            return Json(conteos);
        }
    }
}
