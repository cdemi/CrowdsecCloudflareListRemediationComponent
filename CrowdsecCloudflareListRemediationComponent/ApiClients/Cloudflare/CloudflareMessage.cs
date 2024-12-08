namespace CrowdsecCloudflareListRemediationComponent.ApiClients.Cloudflare
{
    public record CloudflareMessage
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

}
