using GoodfoodSkipper.GoodfoodApi;
using GoodfoodSkipper.GoodfoodApi.Models;
using GoodfoodSkipper.GoodfoodApi.Requests;
using Microsoft.Extensions.Logging;
using System.Reactive.Subjects;

namespace GoodfoodSkipper
{
    internal class DeliverySkipSuccessEvent
    {
        public DeliverySkipSuccessEvent(DateTime deliveryDate)
        {
            DeliveryDate = deliveryDate;
        }

        public DateTime DeliveryDate { get; private set; }
    }

    internal class DeliverySkipFailureEvent
    {
        public DeliverySkipFailureEvent(DateTime deliveryDate, string error)
        {
            DeliveryDate = deliveryDate;
            Error = error;
        }

        public DateTime DeliveryDate { get; private set; }
        public string Error { get; private set; }
    }

    internal class GoodfoodDeliverySkipper
    {
        public IObservable<DeliverySkipSuccessEvent> DeliverySkipSuccess => _successSubject;
        public IObservable<DeliverySkipFailureEvent> DeliverySkipFailure => _failureSubject;

        private readonly GoodfoodAuthProvider _authProvider;
        private readonly ILogger<GoodfoodDeliverySkipper> _logger;
        private readonly GoodfoodApiClient _client;
        private readonly Subject<DeliverySkipSuccessEvent> _successSubject;
        private readonly Subject<DeliverySkipFailureEvent> _failureSubject;

        public GoodfoodDeliverySkipper(GoodfoodApiClient client, GoodfoodAuthProvider authProvider, ILogger<GoodfoodDeliverySkipper> logger)
        {
            _authProvider = authProvider;
            _logger = logger;
            _client = client;
            _successSubject = new Subject<DeliverySkipSuccessEvent>();
            _failureSubject = new Subject<DeliverySkipFailureEvent>();
        }

        public async Task SkipAllDeliveries()
        {
            _logger.LogInformation("Fetching items from cart");
            var authSession = await _authProvider.GetAuthSession();
            var deliveries = await _client.GetCart(new GetCartRequest(authSession.UserId));
            _logger.LogInformation("Found {@cartCount} items in the cart", deliveries.Items.Count);
            foreach(var item in deliveries.Items)
            {
                if (!item.IsActive)
                {
                    _logger.LogInformation("Delivery is already skipped for {@deliveryDate}", item.DeliveryDate);
                    continue;
                };
                await SkipDelivery(authSession.UserId, item);
            }
        }


        private async Task SkipDelivery(string userId, CartItem item)
        {
            _logger.LogInformation("Skipping item delivery for {@deliveryDate}", item.DeliveryDate);
            try
            {
                var response = await _client.SkipDelivery(new SkipDeliveryRequest(userId, item.DeliveryDate));

                if(response.Success)
                {
                    _logger.LogInformation("Successfully skipped item delivery for {@deliveryDate}", item.DeliveryDate);
                    _successSubject.OnNext(new DeliverySkipSuccessEvent(item.DeliveryDate));
                }
                else
                {
                    _logger.LogError("Error skipping delivery for {@deliveryDate}", item.DeliveryDate);
                    _failureSubject.OnNext(new DeliverySkipFailureEvent(item.DeliveryDate, "Unknown error"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error skipping delivery for {@deliveryDate}", item.DeliveryDate);
                _failureSubject.OnNext(new DeliverySkipFailureEvent(item.DeliveryDate, ex.Message));
            }
        }
    }
}
