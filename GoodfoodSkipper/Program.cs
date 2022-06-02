using GoodfoodSkipper;
using GoodfoodSkipper.GoodfoodApi;
using GoodfoodSkipper.GoogleIdentity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, builder) =>
    {
        builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets(typeof(Program).Assembly, true);
    })
    .ConfigureLogging(log =>
    {
        log.AddConsole();
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddLogging(options =>
        {
            options.AddConsole();
        });
        services.Configure<GoodfoodAuthOptions>(ctx.Configuration.GetSection("GoodfoodAuth"));
        services.Configure<DiscordOptions>(ctx.Configuration.GetSection("Discord"));
        services.AddHttpClient<GoodfoodApiClient>();
        services.AddHttpClient<GoogleIdentityToolkitClient>();
        services.AddSingleton<GoodfoodAuthProvider>();
        services.AddSingleton<GoodfoodDeliverySkipper>();
    })
    .UseConsoleLifetime()
    .Build();

await host.StartAsync();
var observerSubscriptions = new List<IDisposable>();
var skipper = host.Services.GetRequiredService<GoodfoodDeliverySkipper>();
var discordOptions = host.Services.GetRequiredService<IOptions<DiscordOptions>>();

if(!string.IsNullOrEmpty(discordOptions.Value.WebhookUrl))
{
    observerSubscriptions.Add(
        skipper.DeliverySkipSuccess.Subscribe(successEvent =>
        {
            var client = new Discord.Webhook.DiscordWebhookClient(discordOptions.Value.WebhookUrl);
            client.SendMessageAsync($"Skipped delivery for {successEvent.DeliveryDate.ToString("yyyy-MM-dd")}");
        }));

    observerSubscriptions.Add(
        skipper.DeliverySkipFailure.Subscribe(failureEvent =>
        {
            var client = new Discord.Webhook.DiscordWebhookClient(discordOptions.Value.WebhookUrl);
            client.SendMessageAsync($"Error skipping delivery for {failureEvent.DeliveryDate.ToString("yyyy-MM-dd")}");
        }));
}


await skipper.SkipAllDeliveries();

foreach (var subscription in observerSubscriptions)
    subscription?.Dispose();

await host.StopAsync();

