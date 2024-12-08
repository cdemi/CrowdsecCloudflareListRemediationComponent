namespace CrowdsecCloudflareListRemediationComponent.ApiClients.Cloudflare
{
    public record Response<T>
    {
        public required CloudflareMessage[] Errors { get; set; }
        public required CloudflareMessage[] Messages { get; set; }
        public T? Result { get; set; }
        public bool Success { get; set; }
        public ResultInfo? ResultInfo { get; set; }
    }
}
