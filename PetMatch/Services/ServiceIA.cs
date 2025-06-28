using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using NuGet.Protocol;
using OpenAI.Chat;
using PetMatch.Models;
using PetMatch.Models.DTO;

public class ServiceIA
{
    private readonly AzureOpenAIClient _client;
    private readonly string _deploymentName;

    /*public IAService()
{
    string endpoint = "";
    string apiKey = "";

    _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
}*/

    public ServiceIA(AzureOpenAIClient client,
                     IOptions<AzureOpenAIConfig> cfg)
    {
        _client = client;
        _deploymentName = cfg.Value.DeploymentName;
    }

    public async Task<string> ObtenerRespuestaTextoAsync(string estiloDeVida)
    {
        var prompt = @$"
            Eres un asistente que analiza un texto que describe el estilo de vida de una persona y devuelve un JSON con esta estructura exacta:

            {{
              ""EtiquetaGenerada"": ""una etiqueta corta que resume el perfil (ejemplo: 'Activo, con niños pequeños')"",
              ""MascotasRecomendadasIds"": [lista de enteros con IDs de mascotas recomendadas]
            }}

            El texto a analizar es:
            ""{estiloDeVida}""

            Solo responde con el JSON sin texto adicional.";

        var chatClient = _client.GetChatClient(_deploymentName);

        var messages = new List<ChatMessage>
    {
        new SystemChatMessage("You are a helpful assistant."),
        new UserChatMessage(prompt)
    };

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 300,
            Temperature = 0.7f
        };

        var response = await chatClient.CompleteChatAsync(messages, options);

        var resul = response.ToJToken();
        string texto = resul.SelectToken("Value.Content[0].Text")?.ToString();

        return texto ?? "No se recibió texto en la respuesta.";
    }


    // Método que devuelve el objeto parseado
    public async Task<RecomendacionDTO> ObtenerRespuestaObjetoAsync(string prompt)
    {
        string texto = await ObtenerRespuestaTextoAsync(prompt);

        try
        {
            var resultado = System.Text.Json.JsonSerializer.Deserialize<RecomendacionDTO>(texto);
            return resultado ?? new RecomendacionDTO();
        }
        catch (Exception)
        {
            // Aquí puedes manejar error de parseo o devolver un resultado vacío
            return new RecomendacionDTO();
        }
    }
}
