using GoodfoodSkipper.GoodfoodApi;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoodfoodSkipper.GoogleIdentity
{
    internal class GoogleIdentityToolkitClient
    {
        private readonly HttpClient _client;
        private readonly string _apiKey;

        private static string SignInWithPasswordUrlTemplate => "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={0}";
        private static string SignInWithPasswordUrl(string apiKey) => string.Format(SignInWithPasswordUrlTemplate, apiKey);

        private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GoogleIdentityToolkitClient(HttpClient client, IOptions<GoodfoodAuthOptions> options)
        {
            _client = client;
            _apiKey = options.Value.IdentityToolkitApiKey;
        }

        public async Task<VerifyPasswordResponse> VerifyPassword(VerifyPasswordRequest req)
        {

            var signInResponse = await _client.PostAsJsonAsync(
                SignInWithPasswordUrl(_apiKey),
                req,
                _jsonSerializerOptions);

            return await signInResponse.Content.ReadFromJsonAsync<VerifyPasswordResponse>(_jsonSerializerOptions);
        }
    }

    public class VerifyPasswordResponse
    {
        public string LocalId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string IdToken { get; set; }
        public bool Registered { get; set; }
        public string RefreshToken { get; set; }
        public string ExpiresIn { get; set; }
    }

    public class VerifyPasswordRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool ReturnSecureToken { get; set; }
    }
}
