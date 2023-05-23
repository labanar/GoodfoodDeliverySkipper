using GoodfoodSkipper;
using GoodfoodSkipper.GoodfoodApi;
using GoodfoodSkipper.GoogleIdentity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Grafana.Loki;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(BuildLogger(builder.Configuration));
builder.Services.Configure<GoodfoodAuthOptions>(builder.Configuration.GetSection("GoodfoodAuth"));
builder.Services.Configure<DiscordOptions>(builder.Configuration.GetSection("Discord"));
builder.Services.AddHttpClient<GoodfoodApiClient>();
builder.Services.AddHttpClient<GoogleIdentityToolkitClient>();
builder.Services.AddSingleton<GoodfoodAuthProvider>();
builder.Services.AddSingleton<GoodfoodDeliverySkipper>();
builder.Services.AddHostedService<DeliverySkipBackgroundService>();

var app = builder.Build();
app.Run();


static Logger BuildLogger(IConfiguration configuration)
{
    var lokiUrl = configuration.GetValue<string>("GrafanaLokiUrl");
    var config = new LoggerConfiguration();
    config.WriteTo.Console();

    if (!string.IsNullOrEmpty(lokiUrl))
        config.WriteTo.GrafanaLoki(lokiUrl, new LokiLabel[]
        {
            new()
            {
                Key = "app_name",
                Value = "goodfood_skipper"
            }
        });

    return config.CreateLogger();
}

