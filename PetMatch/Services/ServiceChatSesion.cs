using System.Collections.Concurrent;

public interface ISesionChatService
{
    void EstablecerReceptor(int emisorId, int receptorId);
    int? ObtenerReceptor(int emisorId);
}

public class ServiceChatSession : ISesionChatService
{
    private readonly ConcurrentDictionary<int, int> _sesiones = new();

    public void EstablecerReceptor(int emisorId, int receptorId)
    {
        _sesiones[emisorId] = receptorId;
    }

    public int? ObtenerReceptor(int emisorId)
    {
        return _sesiones.TryGetValue(emisorId, out var receptorId) ? receptorId : null;
    }
}
