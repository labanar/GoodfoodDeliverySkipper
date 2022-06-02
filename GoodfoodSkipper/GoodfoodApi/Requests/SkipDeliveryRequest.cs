namespace GoodfoodSkipper.GoodfoodApi.Requests
{
    internal class SkipDeliveryRequest
    {
        public string ResourcePath => $"https://api.makegoodfood.ca/cart/user/{UserId}/skip";
        public string DeliveryDate { get; private set; }
        public string UserId { get; private set; }
        public SkipDeliveryRequest(string userId, DateTime deliveryDate)
        {
            UserId = userId;
            DeliveryDate = deliveryDate.ToString("yyyy-MM-dd");
        }
    }
}
