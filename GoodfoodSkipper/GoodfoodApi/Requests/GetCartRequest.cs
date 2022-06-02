namespace GoodfoodSkipper.GoodfoodApi.Requests
{
    internal class GetCartRequest
    {
        public string ResourcePath => $"https://api.makegoodfood.ca/user/{UserId}/cart";
        public string UserId { get; set; }
        public GetCartRequest(string userId)
        {
            UserId = userId;
        }
    }
}
