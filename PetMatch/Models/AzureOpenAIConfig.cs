namespace PetMatch.Models
{
    public class AzureOpenAIConfig
    {
        public string Endpoint { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
        public string DeploymentName { get; set; } = null!;
    }

}
