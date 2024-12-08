namespace CrowdsecCloudflareListRemediationComponent.ApiClients.Cloudflare
{
    public record CreateListItem
    {
        public string Ip { get; set; }
        public string Comment { get; set; }
    }

}
