namespace CrowdsecCloudflareListRemediationComponent.Configurations
{
    public record Crowdsec
    {
        public const string ConfigurationSection = "Crowdsec";
        public required string LapiUrl {  get; set; }
        public required string LapiKey {  get; set; }
        public required TimeSpan PollingFrequency { get; set; } = TimeSpan.FromSeconds(10);
    }
}
