using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetMatch.Context;

public class ChatController : Controller
{
    private readonly AppDbContext _context;

    public ChatController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerMensajes(int receptorId)
    {
        int emisorId = int.Parse(User.FindFirst("UsuarioId")!.Value);

        var mensajes = await _context.Mensajes
            .Where(m =>
                (m.EmisorId == emisorId && m.ReceptorId == receptorId) ||
                (m.EmisorId == receptorId && m.ReceptorId == emisorId))
            .OrderBy(m => m.FechaEnvio)
            .Select(m => new
            {
                contenido = m.Contenido,
                fecha = m.FechaEnvio.ToString("g"),
                emisor = m.EmisorId
            })
            .ToListAsync();

        return Json(mensajes); // <-- NO RENDERIZA VISTA, solo datos
    }

    public async Task<IActionResult> MisChats()
    {
        var userIdClaim = User.FindFirst("UserId");
        if (userIdClaim == null)
            return BadRequest("No se encontró el UserId en los claims.");

        int usuarioId = int.Parse(userIdClaim.Value);

        var mensajes = await _context.Mensajes
            .Include(m => m.Emisor)
            .Include(m => m.Receptor)
            .Where(m => m.EmisorId == usuarioId || m.ReceptorId == usuarioId)
            .ToListAsync();

        // Agrupar en memoria
        var chats = mensajes
            .GroupBy(m => m.EmisorId == usuarioId ? m.ReceptorId : m.EmisorId)
            .Select(g => g.OrderByDescending(m => m.FechaEnvio).First())
            .ToList();

        return View(chats);
    }

}
