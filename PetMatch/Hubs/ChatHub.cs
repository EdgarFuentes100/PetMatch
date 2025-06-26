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
        int emisorId = int.Parse(Context.User.FindFirst("UserId")!.Value);

        var esContactoValido = _context.Usuario.Any(u => u.UsuarioId == receptorId /* y otras validaciones */);

        if (!esContactoValido)
            throw new HubException("Usuario inválido");

        _sesionChat.EstablecerReceptor(emisorId, receptorId);
        return Task.CompletedTask;
    }


    public async Task<List<MensajeDTO>> ObtenerHistorial(int receptorId)
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
                EsPropio = m.EmisorId == emisorId
            })
            .ToListAsync();

        return mensajes;
    }

    public async Task EnviarMensaje(string mensaje)
    {
        int emisorId = int.Parse(Context.User.FindFirst("UserId")!.Value);
        var receptorId = _sesionChat.ObtenerReceptor(emisorId);

        if (receptorId == null)
            return;

        // Guardar mensaje en base de datos
        var nuevoMensaje = new Mensaje
        {
            EmisorId = emisorId,
            ReceptorId = receptorId.Value,
            Contenido = mensaje,
            FechaEnvio = DateTime.UtcNow
        };

        _context.Mensajes.Add(nuevoMensaje);
        await _context.SaveChangesAsync();

        // Enviar a clientes
        await Clients.User(receptorId.ToString()).SendAsync("RecibirMensaje", emisorId, mensaje, false);
        await Clients.User(emisorId.ToString()).SendAsync("RecibirMensaje", emisorId, mensaje, true);
    }
}
