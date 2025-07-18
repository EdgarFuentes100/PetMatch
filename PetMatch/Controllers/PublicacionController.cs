using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using PetMatch.Context;
using PetMatch.Models;
using PetMatch.Services;
using System.IO;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PetMatch.Controllers
{
    public class PublicacionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ServiceTransactionHelper _tx;

        public PublicacionController(AppDbContext context, IWebHostEnvironment env, ServiceTransactionHelper tx)
        {
            _context = context;
            _env = env;
            _tx = tx;
        }

        // ✅ Vista de publicaciones del usuario
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

        [HttpPost("Crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(IFormFile Foto,string Titulo,
         DateTime FechaPublicacion,
         string Historia,
         string Nombre,
         int EdadMeses,
         string Sexo, // ← cambiado
         string Tamano,
         string Temperamento,
         int NivelEnergia,
         bool Sociable,
         bool RequiereEjercicio,
         string Raza,
         string Descripcion,
         bool CompatibleConNiños,
         bool CompatibleConOtrasMascotas,
         int NivelCuidado,
         string AmbienteIdeal,
         int TipoMascotaId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized();

                string fotoUrl = null;
                if (Foto != null)
                {
                    string carpeta = Path.Combine(_env.WebRootPath, "images", "mascotas");
                    if (!Directory.Exists(carpeta))
                        Directory.CreateDirectory(carpeta);

                    string fileName = Guid.NewGuid() + Path.GetExtension(Foto.FileName);
                    string path = Path.Combine(carpeta, fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await Foto.CopyToAsync(stream);
                    }
                    fotoUrl = "/images/mascotas/" + fileName;
                }

                var mascota = new Mascota
                {
                    Nombre = Nombre,
                    EdadMeses = EdadMeses,
                    Sexo = string.IsNullOrEmpty(Sexo) ? 'M' : Sexo[0],
                    Tamano = Tamano,
                    FotoUrl = fotoUrl,
                    Temperamento = Temperamento ?? "",
                    NivelEnergia = NivelEnergia,
                    Sociable = Sociable,
                    RequiereEjercicio = RequiereEjercicio,
                    Raza = Raza ?? "",
                    Descripcion = Descripcion ?? "",
                    CompatibleConNiños = CompatibleConNiños,
                    CompatibleConOtrasMascotas = CompatibleConOtrasMascotas,
                    NivelCuidado = NivelCuidado,
                    AmbienteIdeal = AmbienteIdeal ?? "",
                    TipoMascotaId = TipoMascotaId
                };

                _context.Mascotas.Add(mascota);
                await _context.SaveChangesAsync();

                // ✅ 4. Crear Publicación
                var publicacion = new Publicacion
                {
                    Titulo = Titulo,
                    FechaPublicacion = FechaPublicacion,
                    Historia = Historia,
                    MascotaId = mascota.MascotaId,
                    PublicadorId = int.Parse(userIdClaim)
                };

                _context.Publicaciones.Add(publicacion);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToAction("MisPublicaciones");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error: {ex.Message} | {ex.InnerException?.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RecargarBloque(string bloque, int mascotaId)
        {
            try
            {
                var solicitudes = await _context.Solicitud
                    .Include(s => s.Solicitante)
                    .Where(s => s.MascotaId == mascotaId)
                    .ToListAsync();

                switch (bloque)
                {
                    case "pendientes":
                        return PartialView("Partials/_SolicitudesPendientes", solicitudes.Where(s => s.Estado == 'P'));
                    case "aceptadas":
                        return PartialView("Partials/_SolicitudesAceptadas", solicitudes.Where(s => s.Estado == 'A'));
                    default:
                        return BadRequest("Bloque inválido");
                }
            }
            catch (Exception ex)
            {
                // Para desarrollo, devolver el mensaje de error directamente (no usar en producción)
                return Content($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public IActionResult HacerPublicacion()
        {
            return View();
        }
    }
}
