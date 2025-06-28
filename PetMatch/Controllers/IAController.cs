using Azure;
using Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using PetMatch.Context;
using PetMatch.Models;
using PetMatch.Models.DTO;
using PetMatch.Services;
using System.Threading.Tasks;


//[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
//Este atributo evita que la respuesta se guarde en caché. Es útil en páginas donde hay datos sensibles o personalizados por usuario.
[Authorize(Policy = "All")]
public class IAController : Controller
{
    private readonly ServiceIA _iaService;

    private readonly AppDbContext _context;

    public IAController(ServiceIA iaService, AppDbContext context)
    {
        _iaService = iaService;
        _context = context;
    }

    // GET: muestra el formulario
    [HttpGet]
    public IActionResult HacerMatch()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HacerMatch(string descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            ModelState.AddModelError(string.Empty, "La descripción no puede estar vacía.");
            return View();
        }

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var perfilExistente = await _context.PerfilMascotaIdeal
            .Include(p => p.MascotasIdeales)
            .FirstOrDefaultAsync(p => p.UsuarioId == userId && p.TextoEstiloVida == descripcion);

        if (perfilExistente != null)
        {
            // Perfil ya existe, cargar resultados desde BD sin llamar a la IA
            var mascotasRecomendadas = await _context.Mascotas
                .Where(m => perfilExistente.MascotasIdeales.Select(mi => mi.MascotaId).Contains(m.MascotaId))
                .ToListAsync();

            ViewBag.MascotasRecomendadas = mascotasRecomendadas;
            ViewBag.EtiquetaGenerada = perfilExistente.EtiquetaGenerada;

            return View();
        }

        // Si no existe perfil, llamar a IA para obtener respuesta
        RecomendacionDTO resultado = await _iaService.ObtenerRespuestaObjetoAsync(descripcion);

        var perfil = new PerfilMascotaIdeal
        {
            UsuarioId = userId,
            TextoEstiloVida = descripcion,
            EtiquetaGenerada = resultado.EtiquetaGenerada,
            TiposMascotaIdeales = await ObtenerTiposMascotaDesdeIds(resultado.MascotasRecomendadasIds)
        };

        _context.PerfilMascotaIdeal.Add(perfil);
        await _context.SaveChangesAsync();

        foreach (var mascotaId in resultado.MascotasRecomendadasIds)
        {
            _context.MascotaIdeal.Add(new MascotaIdeal
            {
                PerfilId = perfil.PerfilMascotaIdealId,
                MascotaId = mascotaId
            });
        }
        await _context.SaveChangesAsync();

        var mascotas = await _context.Mascotas
            .Where(m => resultado.MascotasRecomendadasIds.Contains(m.MascotaId))
            .ToListAsync();

        ViewBag.MascotasRecomendadas = mascotas;
        ViewBag.EtiquetaGenerada = resultado.EtiquetaGenerada;

        return View();
    }

    private async Task<string> ObtenerTiposMascotaDesdeIds(IEnumerable<int> mascotaIds)
    {
        var tipos = await _context.Mascotas
            .Where(m => mascotaIds.Contains(m.MascotaId))
            .Select(m => m.TipoMascota.Nombre)
            .Distinct()
            .ToListAsync();

        return string.Join(", ", tipos);
    }
}