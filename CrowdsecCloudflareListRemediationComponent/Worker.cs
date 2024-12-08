using CrowdsecCloudflareListRemediationComponent.ApiClients.Cloudflare;
using CrowdsecCloudflareListRemediationComponent.ApiClients.Crowdsec;
using CrowdsecCloudflareListRemediationComponent.Configurations;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Net;

namespace CrowdsecCloudflareListRemediationComponent
{
    public class Worker(ILogger<Worker> logger,
                        IOptions<Cloudflare> cloudflareConfiguration,
                        IOptions<Crowdsec> crowdsecConfiguration,
                        ICloudflareApi cloudflareApi,
                        ICrowdsecLapiApi crowdsecLapi) : BackgroundService
    {
        private Dictionary<string, string?> cloudflareIPs = [];
        static readonly AsyncRetryPolicy retryPolicy = Policy
               .Handle<Exception>()
               .WaitAndRetryForeverAsync(
                                retryAttempt => TimeSpan.FromSeconds(Math.Min(60, retryAttempt)) // Capped exponential backoff
                            );
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cloudflareIPs = await GetCloudflareIPsInList(stoppingToken);

            bool startup = true;
            while (!stoppingToken.IsCancellationRequested)
            {
                var decisionsDelta = await retryPolicy.ExecuteAsync((stoppingToken) => crowdsecLapi.DecisionsStream(startup, stoppingToken), stoppingToken);
                startup = false;

                if (decisionsDelta.Deleted is not null)
                {
                    HashSet<string> ipsToDelete = decisionsDelta.Deleted.Select(d => d with { Value = ConvertIPv6ToCidr(d.Value) }).Select(i => i.Value).ToHashSet();
                    var listItemIdsToDelete = cloudflareIPs.Where(c => ipsToDelete.Contains(c.Key)).Select(c => c.Value);
                    if (listItemIdsToDelete.Where(l=> l==null).Any())
                    {
                        cloudflareIPs = await GetCloudflareIPsInList(stoppingToken);
                        listItemIdsToDelete = cloudflareIPs.Where(c => ipsToDelete.Contains(c.Key)).Select(c => c.Value);
                    }

                    if (listItemIdsToDelete.Any())
                    {
                        var s = await retryPolicy.ExecuteAsync((stoppingToken) => cloudflareApi.DeleteListItems(cloudflareConfiguration.Value.AccountId, cloudflareConfiguration.Value.ListId, new DeleteListItemsRequest { Items = listItemIdsToDelete.Select(i => new DeleteListItem { Id = i }).ToList() }, stoppingToken), stoppingToken);
                        foreach (var listItemId in listItemIdsToDelete)
                        {
                            cloudflareIPs.Remove(listItemId);
                        }
                    }
                }

                if (decisionsDelta.New is not null)
                {
                    var listItemsToAdd = decisionsDelta.New
                        .Where(d => !cloudflareIPs.ContainsKey(d.Value))
                        .Select(d=> d with { Value = ConvertIPv6ToCidr(d.Value) })
                        .DistinctBy(d=>d.Value)
                        .Select(d => new CreateListItem
                        {
                            Ip = d.Value,
                            Comment = $"Decision {d.Id}"
                        });

                    if (listItemsToAdd.Any())
                    {
                        var s = await retryPolicy.ExecuteAsync((stoppingToken) => cloudflareApi.CreateListItems(cloudflareConfiguration.Value.AccountId, cloudflareConfiguration.Value.ListId, listItemsToAdd.ToList(), stoppingToken), stoppingToken);

                        foreach (var listItemId in listItemsToAdd)
                        {
                            cloudflareIPs.Add(listItemId.Ip, null);
                        }
                    }
                }

                await Task.Delay(crowdsecConfiguration.Value.PollingFrequency, stoppingToken);
            }
        }

        private async Task<Dictionary<string, string>> GetCloudflareIPsInList(CancellationToken stoppingToken)
        {
            var ips = new Dictionary<string, string>();

            string? cursor = null;

            do
            {
                var response = await retryPolicy.ExecuteAsync((stoppingToken) => cloudflareApi.GetListItems(cloudflareConfiguration.Value.AccountId, cloudflareConfiguration.Value.ListId, cursor, cancellationToken: stoppingToken), stoppingToken);
                foreach (var result in response.Result!)
                {
                    ips.Add(result.Ip, result.Id);
                }
                cursor = response.ResultInfo?.Cursors.After;
            }
            while (cursor is not null);

            return ips;
        }

        private static string ConvertIPv6ToCidr(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out var ip))
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) // IPv6
                {
                    // Convert to /64 CIDR by zeroing out the last 64 bits
                    var bytes = ip.GetAddressBytes();

                    for (int i = 8; i < bytes.Length; i++) // Zero out the last 8 bytes (64 bits)
                    {
                        bytes[i] = 0;
                    }

                    var cidrAddress = new IPAddress(bytes).ToString();
                    return $"{cidrAddress}/64";
                }
                else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4
                {
                    // Return the IPv4 address as is
                    return ipAddress;
                }
                else
                {
                    throw new ArgumentException("Unsupported IP address type.");
                }
            }
            else
            {
                throw new ArgumentException("Invalid IP address format.");
            }
        }
    }
}
