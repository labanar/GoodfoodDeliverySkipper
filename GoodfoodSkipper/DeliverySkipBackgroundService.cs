using GoodfoodSkipper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal class DeliverySkipBackgroundService : BackgroundService
{
    private readonly GoodfoodDeliverySkipper _skipper;
    private readonly IOptions<DiscordOptions> _discordOptions;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public DeliverySkipBackgroundService(GoodfoodDeliverySkipper skipper, IOptions<DiscordOptions> discordOptions, IHostApplicationLifetime applicationLifetime)
    {
        _skipper = skipper;
        _discordOptions = discordOptions;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var observerSubscriptions = new List<IDisposable>();
        if (!string.IsNullOrEmpty(_discordOptions?.Value?.WebhookUrl))
        {
            observerSubscriptions.Add(
                _skipper.DeliverySkipSuccess.Subscribe(successEvent =>
                {
                    var client = new Discord.Webhook.DiscordWebhookClient(_discordOptions.Value.WebhookUrl);
                    client.SendMessageAsync($"Skipped delivery for {successEvent.DeliveryDate.ToString("yyyy-MM-dd")}");
                }));

            observerSubscriptions.Add(
                _skipper.DeliverySkipFailure.Subscribe(failureEvent =>
                {
                    var client = new Discord.Webhook.DiscordWebhookClient(_discordOptions.Value.WebhookUrl);
                    client.SendMessageAsync($"Error skipping delivery for {failureEvent.DeliveryDate.ToString("yyyy-MM-dd")}");
                }));
        }

        await _skipper.SkipAllDeliveries();
        foreach (var subscription in observerSubscriptions)
            subscription?.Dispose();

        await Task.Delay(5_000);
        _applicationLifetime.StopApplication();
    }
}


