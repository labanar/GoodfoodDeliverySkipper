using GoodfoodSkipper.GoodfoodApi.Models;

namespace GoodfoodSkipper.GoodfoodApi.Responses
{
    internal class GetCartResponse
    {
        public ICollection<CartItem> Items { get; set; }
    }
}
