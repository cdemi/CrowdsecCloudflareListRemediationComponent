namespace CrowdsecCloudflareListRemediationComponent.ApiClients.Cloudflare
{
    public record DeleteListItemsRequest
    {
        public required List<DeleteListItem> Items { get; set; }
    }
}
