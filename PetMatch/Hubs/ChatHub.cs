using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PetMatch.Context;
using PetMatch.Models;
using PetMatch.Models.DTO;
using System.Security.Claims;

[Authorize]
public class ChatHub : Hub
{
    private readonly ISesionChatService _sesionChat;
    private readonly AppDbContext _context;

    public ChatHub(ISesionChatService sesionChat, AppDbContext context)
    {
        _sesionChat = sesionChat;
        _context = context;
    }

    public Task SeleccionarReceptor(int receptorId)
    {
        int emisorId = ObtenerUserId();

        var esContactoValido = _context.Usuario.Any(u => u.UsuarioId == receptorId);
        if (!esContactoValido)
            throw new HubException("Usuario inválido");

        _sesionChat.EstablecerReceptor(emisorId, receptorId);
        return Task.CompletedTask;
    }

    public Task CerrarChat()
    {
        int emisorId = ObtenerUserId();
        _sesionChat.CerrarChat(emisorId);
        return Task.CompletedTask;
    }

    public async Task<object> ObtenerHistorial(int receptorId)
    {
        int emisorId = int.Parse(Context.User.FindFirst("UserId")!.Value);

        var mensajes = await _context.Mensajes
            .Where(m =>
                (m.EmisorId == emisorId && m.ReceptorId == receptorId) ||
                (m.EmisorId == receptorId && m.ReceptorId == emisorId))
            .OrderBy(m => m.FechaEnvio)
            .Take(100)
            .Select(m => new MensajeDTO
            {
                Contenido = m.Contenido,
                FechaEnvio = m.FechaEnvio,
                EsPropio = m.EmisorId == emisorId,
                Leido = m.Leido
            })
            .ToListAsync();

        return new
        {
            userId = emisorId,
            mensajes
        };
    }


    public async Task EnviarMensaje(string mensaje)
    {
        int emisorId = ObtenerUserId();
        int? receptorId = _sesionChat.ObtenerReceptor(emisorId);

        if (receptorId == null)
            return;

        // Verificar si el receptor tiene abierto el chat con el emisor
        bool receptorTieneChatAbierto = _sesionChat.ObtenerReceptor(receptorId.Value) == emisorId;

        var nuevoMensaje = new Mensaje
        {
            EmisorId = emisorId,
            ReceptorId = receptorId.Value,
            Contenido = mensaje,
            FechaEnvio = DateTime.UtcNow,
            Leido = receptorTieneChatAbierto
        };

        _context.Mensajes.Add(nuevoMensaje);
        await _context.SaveChangesAsync();

        // Enviar mensaje a ambos usuarios
        await Clients.User(receptorId.ToString()).SendAsync("RecibirMensaje", emisorId, mensaje, false, receptorTieneChatAbierto);
        await Clients.User(emisorId.ToString()).SendAsync("RecibirMensaje", emisorId, mensaje, true, receptorTieneChatAbierto);

        // Notificar al emisor si el receptor tenía el chat abierto (ya leído)
        if (receptorTieneChatAbierto)
        {
            await Clients.User(emisorId.ToString()).SendAsync("ActualizarEstadoLeidos", receptorId);
        }

        // Notificar receptor y emisor para actualizar lista de chats
        await Clients.User(receptorId.ToString()).SendAsync("RecibirMensajeNuevo");
        await Clients.User(emisorId.ToString()).SendAsync("RecibirMensajeNuevo");
    }

    public async Task<List<MensajeResumenDTO>> ObtenerMisChats()
    {
        var usuarioId = ObtenerUserId();

        var mensajes = await _context.Mensajes
            .Include(m => m.Emisor)
            .Include(m => m.Receptor)
            .Where(m => m.EmisorId == usuarioId || m.ReceptorId == usuarioId)
            .ToListAsync();

        var chats = mensajes
            .GroupBy(m => m.EmisorId == usuarioId ? m.ReceptorId : m.EmisorId)
            .Select(g => g.OrderByDescending(m => m.FechaEnvio).First())
            .Select(m => new MensajeResumenDTO
            {
                EmisorId = m.EmisorId,
                ReceptorId = m.ReceptorId,
                Contenido = m.Contenido,
                Leido = m.Leido,
                OtroUsuarioId = m.EmisorId == usuarioId ? m.Receptor.UsuarioId : m.Emisor.UsuarioId,
                OtroNombre = m.EmisorId == usuarioId ? m.Receptor.Nombre : m.Emisor.Nombre
            })
            .ToList();

        return chats;
    }

    public async Task MarcarMensajesComoLeidos(int emisorId, int receptorId)
    {
        // El receptor actual es quien está abriendo el chat
        var mensajesNoLeidos = await _context.Mensajes
            .Where(m => m.EmisorId == emisorId && m.ReceptorId == receptorId && !m.Leido)
            .ToListAsync();

        if (mensajesNoLeidos.Any())
        {
            foreach (var mensaje in mensajesNoLeidos)
            {
                mensaje.Leido = true;
            }

            await _context.SaveChangesAsync();

            // Notificar al emisor original (quien envió) que sus mensajes fueron leídos
            await Clients.User(emisorId.ToString()).SendAsync("ActualizarEstadoLeidos", receptorId);

            // Notificar a ambos usuarios que actualicen la lista de chats (para refrescar estados "nuevo mensaje")
            await Clients.User(emisorId.ToString()).SendAsync("ActualizarListaChats");
            await Clients.User(receptorId.ToString()).SendAsync("ActualizarListaChats");

        }
    }

    private int ObtenerUserId()
    {
        string? userIdClaim = Context.User?.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new HubException("Usuario no autenticado");

        return userId;
    }
}
