using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using NuGet.Protocol;
using OpenAI.Chat;
using PetMatch.Models;

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

    public async Task<string> ObtenerRespuestaAsync(string prompt)
    {
        try
        {
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

            string texto = resul
                .SelectToken("Value.Content[0].Text")?
                .ToString();

            return texto ?? "No se recibió texto en la respuesta.";
        }
        catch (RequestFailedException ex) when (ex.Status == 429)
        {
            return "Límite alcanzado o sin tokens disponibles. Intenta más tarde.";
        }
        // 2️⃣ Cualquier otro error
        catch (Exception ex)
        {
            // Puedes registrar 'ex' con ILogger aquí si lo deseas
            return $"Ocurrió un error inesperado: {ex.Message}";
        }
    }
}
