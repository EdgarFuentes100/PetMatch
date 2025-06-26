using System.Collections.Concurrent;

public interface ISesionChatService
{
    void EstablecerReceptor(int emisorId, int receptorId);
    int? ObtenerReceptor(int emisorId);
    void CerrarChat(int emisorId);
    bool EstaChatAbierto(int receptorId, int emisorId);
}

public class ServiceChatSession : ISesionChatService
{
    // Diccionario que guarda el receptor actual de cada usuario
    private readonly ConcurrentDictionary<int, int> _sesiones = new();

    public void EstablecerReceptor(int emisorId, int receptorId)
    {
        _sesiones[emisorId] = receptorId;
    }

    public int? ObtenerReceptor(int emisorId)
    {
        return _sesiones.TryGetValue(emisorId, out var receptorId) ? receptorId : null;
    }

    public void CerrarChat(int emisorId)
    {
        _sesiones.TryRemove(emisorId, out _);
    }

    public bool EstaChatAbierto(int receptorId, int emisorId)
    {
        // Retorna true si el receptor tiene seleccionado al emisor como su chat actual
        return _sesiones.TryGetValue(receptorId, out var id) && id == emisorId;
    }
}
