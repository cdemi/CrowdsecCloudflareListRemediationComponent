namespace CrowdsecCloudflareListRemediationComponent.ApiClients.Cloudflare
{
    public record ListItem
    {
        public string Id { get; set; }
        public string Ip { get; set; }
        public string Comment { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset ModifiedOn { get; set; }
    }

}
