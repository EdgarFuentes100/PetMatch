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
