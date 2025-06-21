using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetMatch.Context;
using PetMatch.Models;
using System.Security.Claims;

namespace PetMatch.Controllers
{
    [Authorize] // ← Esto protege toda la clase
    public class PerfilController : Controller
    {
        private readonly AppDbContext _context;

        public PerfilController(AppDbContext appContext) {
            _context = appContext;
        }
        // GET: PerfilController
        public async Task<IActionResult> VistaPerfil()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var perfil = await _context.Perfil
                .Include(p => p.Usuario)
                .ThenInclude(r => r.Rol)
                .FirstOrDefaultAsync(p => p.Usuario.Email == email);

            return View(perfil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPerfil(Perfil perfilEditado)
        {
            // Obtener el Id de usuario del claim
            var userIdString = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdString, out int userId))
                return BadRequest("Usuario no identificado");

            // Asignar el Id al objeto que viene del formulario
            perfilEditado.UsuarioId = userId;

            // Marcar el objeto como modificado para EF
            _context.Entry(perfilEditado).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(VistaPerfil));
        }
    }
}
