namespace CrowdsecCloudflareListRemediationComponent.ApiClients.Crowdsec
{

    public record DecisionStream
    {
        public List<Decision>? Deleted { get; set; }
        public List<Decision>? New { get; set; }
    }

    public record Decision
    {
        public uint Id { get; set; }
        public string Duration { get; set; }
        public string Origin { get; set; }
        public string Scenario { get; set; }
        public string Scope { get; set; }
        public string Type { get; set; }
        public string Uuid { get; set; }
        public string Value { get; set; }
    }

}
