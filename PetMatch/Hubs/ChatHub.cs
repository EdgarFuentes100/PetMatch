using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task EnviarMensaje(string receptorId, string mensaje)
    {
        var emisorId = Context.UserIdentifier;

        // Enviar mensaje al receptor, con esPropio = false
        await Clients.User(receptorId).SendAsync("RecibirMensaje", emisorId, mensaje, false);

        // Enviar mensaje al emisor, con esPropio = true
        await Clients.User(emisorId).SendAsync("RecibirMensaje", emisorId, mensaje, true);
    }
}
