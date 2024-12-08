namespace CrowdsecCloudflareListRemediationComponent.Configurations
{
    public record Cloudflare
    {
        public const string ConfigurationSection = "Cloudflare";
        public string BaseUrl { get; set; } = "https://api.cloudflare.com/client";
        public required string AccountId {  get; set; }
        public required string ApiKey {  get; set; }
        public required string ListId {  get; set; }
    }
}
