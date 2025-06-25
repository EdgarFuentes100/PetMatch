// CustomUserIdProvider.cs
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class CustomUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // Usa el Claim "UsuarioId" como identificador en SignalR
        return connection.User?.FindFirst("UsuarioId")?.Value;
    }
}
