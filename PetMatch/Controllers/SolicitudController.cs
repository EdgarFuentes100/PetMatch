using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using PetMatch.Context;      // tu namespace real
using PetMatch.Models;
using PetMatch.Services;
using System.Drawing;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        // GET: /Adopcion/VistaAdopcion
        [AllowAnonymous] // ✅ ESTA vista sí es pública
        public async Task<IActionResult> VistaAdopcion()
        {
            // 1) Las publicaciones que quieres mostrar
            var publicaciones = await _context.Publicaciones
                .Where(pub => !_context.Solicitud.Any(ado => ado.MascotaId == pub.MascotaId))
                .Include(p => p.Mascota)
                .ThenInclude(m => m.TipoMascota)
                .Include(p => p.Publicador)
                .ToListAsync();

            return View(publicaciones);
        }

        public IActionResult VistaSolicitud(int id)
        {
            var publicacion = _context.Publicaciones
                .Include(p => p.Mascota)
                    .ThenInclude(m => m.TipoMascota)
                .Include(p => p.Publicador)
                .FirstOrDefault(p => p.PublicacionId == id);

            if (publicacion == null)
                return NotFound();

            return View(publicacion); // Vista Adoptar.cshtml
        }

        public async Task<IActionResult> EnviarSolicitud(Solicitud solicitud, int idPublicacion)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                TempData["Mensaje"] = "Por favor, completa todos los campos correctamente.";
                TempData["EsExito"] = false;
                return RedirectToAction(nameof(VistaSolicitud), new { id = idPublicacion });
            }

            await _transactionHelper.RunAsync(async () =>
            {
                solicitud.SolicitanteId = int.Parse(userId);
                solicitud.Estado = 'P';

                _context.Solicitud.Add(solicitud);
                await _context.SaveChangesAsync();
            });

            TempData["Mensaje"] = "✅ Solicitud enviada con éxito.";
            TempData["EsExito"] = true;
            TempData["RedirigirA"] = Url.Action("VistaAdopcion"); // Puedes cambiar esto
            return RedirectToAction(nameof(VistaSolicitud), new { id = idPublicacion });
        }

    }
}
