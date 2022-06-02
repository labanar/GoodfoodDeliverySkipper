using GoodfoodSkipper.GoodfoodApi.Models;
using GoodfoodSkipper.GoodfoodApi.Requests;
using GoodfoodSkipper.GoodfoodApi.Responses;
using System.Text;
using System.Text.Json;

namespace GoodfoodSkipper.GoodfoodApi
{
    internal class GoodfoodApiClient
    {
        private readonly HttpClient _client;
        private readonly GoodfoodAuthProvider _authProvider;
        private static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GoodfoodApiClient(HttpClient client, GoodfoodAuthProvider authProvider)
        {
            _client = client;
            _authProvider = authProvider;
        }


        public async Task<GetCartResponse> GetCart(GetCartRequest req)
        {
            var authSession = await _authProvider.GetAuthSession();
            using var request = new HttpRequestMessage(HttpMethod.Get, req.ResourcePath);
            request.Headers.Add("Authorization", $"Bearer {authSession.IdToken}");
            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var items = JsonSerializer.Deserialize<CartItem[]>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
            return new GetCartResponse
            {
                Items = items ?? Array.Empty<CartItem>()
            };
        }


        public async Task<SkipDeliveryResponse> SkipDelivery(SkipDeliveryRequest req)
        {
            var authSession = await _authProvider.GetAuthSession();
            using var request = new HttpRequestMessage(HttpMethod.Post, req.ResourcePath);
            request.Headers.Add("Authorization", $"Bearer {authSession.IdToken}");
            request.Headers.Add("User-Agent", "Goodfood/iOS");
            request.Content = new StringContent(JsonSerializer.Serialize(new { delivery_date = req.DeliveryDate }, _jsonSerializerOptions), Encoding.UTF8, "application/json");
            using var response = await _client.SendAsync(request);
            return new SkipDeliveryResponse(response.IsSuccessStatusCode);
        }
    }
}
