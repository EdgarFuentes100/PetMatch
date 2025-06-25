// Hubs/ChatHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task EnviarMensaje(string receptorId, string mensaje)
    {
        var emisorId = Context.UserIdentifier;

        // Enviar el mensaje al receptor
        await Clients.User(receptorId).SendAsync("RecibirMensaje", emisorId, mensaje);

        // Enviar también al emisor (para que vea su mensaje reflejado)
        await Clients.User(emisorId).SendAsync("RecibirMensaje", emisorId, mensaje);
    }
}
