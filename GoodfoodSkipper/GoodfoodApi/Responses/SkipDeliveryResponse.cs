using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodfoodSkipper.GoodfoodApi.Responses
{
    internal class SkipDeliveryResponse
    {
        public SkipDeliveryResponse(bool success)
        {
            Success = success;
        }

        public bool Success { get; set; }
    }
}
