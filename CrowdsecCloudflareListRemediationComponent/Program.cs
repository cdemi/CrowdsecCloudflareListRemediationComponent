using CrowdsecCloudflareListRemediationComponent.ApiClients.Cloudflare;
using CrowdsecCloudflareListRemediationComponent.ApiClients.Crowdsec;
using CrowdsecCloudflareListRemediationComponent.Configurations;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;
using System.Net;
using System.Text.Json;

namespace CrowdsecCloudflareListRemediationComponent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.Configure<Cloudflare>(builder.Configuration.GetSection(Cloudflare.ConfigurationSection));
            builder.Services.Configure<Crowdsec>(builder.Configuration.GetSection(Crowdsec.ConfigurationSection));
            builder.Services.AddTransient<LoggingHttpMessageHandler>();
            var refitSettings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        PropertyNameCaseInsensitive = true
                    }
                )
            };

            builder.Services.AddRefitClient<ICloudflareApi>(refitSettings)
                .ConfigureHttpClient((sp, c) =>
                {
                    var cloudflareOptions = sp.GetRequiredService<IOptions<Cloudflare>>().Value;
                    c.BaseAddress = new Uri(cloudflareOptions.BaseUrl);
                    c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cloudflareOptions.ApiKey);
                })
                .AddHttpMessageHandler<LoggingHttpMessageHandler>();

            builder.Services.AddRefitClient<ICrowdsecLapiApi>(refitSettings)
                .ConfigureHttpClient((sp, c) =>
                {
                    var crowdsecOptions = sp.GetRequiredService<IOptions<Crowdsec>>().Value;
                    c.BaseAddress = new Uri(crowdsecOptions.LapiUrl);
                    c.DefaultRequestHeaders.Add("X-Api-Key", crowdsecOptions.LapiKey);
                })
                .AddHttpMessageHandler<LoggingHttpMessageHandler>();

            var host = builder.Build();
            host.Run();
        }
    }
}