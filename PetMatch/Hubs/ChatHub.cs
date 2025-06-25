using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PetMatch.Context;
using PetMatch.Models;
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
        _sesionChat.EstablecerReceptor(emisorId, receptorId);
        return Task.CompletedTask;
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
