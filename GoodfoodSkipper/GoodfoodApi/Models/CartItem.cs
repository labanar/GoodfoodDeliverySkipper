using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodfoodSkipper.GoodfoodApi.Models
{
    internal class CartItem
    {
        public DateTime DeliveryDate
        {
            get
            {
                var part = delivery_date.Split(' ').FirstOrDefault();
                return DateTime.Parse(part);
            }
        }
        public string delivery_date { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string ItemCount { get; set; }
        public bool IsActive => Status.Equals("active", StringComparison.OrdinalIgnoreCase);
    }
}
