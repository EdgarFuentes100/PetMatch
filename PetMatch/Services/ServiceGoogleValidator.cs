using Google.Apis.Auth;

namespace PetMatch.Services
{
    public class ServiceGoogleValidator
    {
        private readonly string _clientId;

        public ServiceGoogleValidator(IConfiguration config)
        {
            _clientId = config["Authentication:Google:ClientId"]; // ← lo obtienes de appsettings
        }

        public async Task<GoogleJsonWebSignature.Payload> ValidarAsync(string idToken)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _clientId }
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
    }
}
