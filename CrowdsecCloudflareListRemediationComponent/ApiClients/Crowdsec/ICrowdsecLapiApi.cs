using Refit;

namespace CrowdsecCloudflareListRemediationComponent.ApiClients.Crowdsec
{
    public interface ICrowdsecLapiApi
    {
        [Get("/v1/decisions/stream?scopes=ip&origins=cscli,crowdsec")]
        Task<DecisionStream> DecisionsStream(bool startup, CancellationToken cancellationToken = default);
    }
}
