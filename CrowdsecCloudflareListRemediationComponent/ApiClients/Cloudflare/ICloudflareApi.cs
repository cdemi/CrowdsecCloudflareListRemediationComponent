using Refit;

namespace CrowdsecCloudflareListRemediationComponent.ApiClients.Cloudflare
{
    public interface ICloudflareApi
    {
        [Get("/v4/accounts/{accountId}/rules/lists/{listId}/items")]
        Task<Response<List<ListItem>>> GetListItems(string accountId, string listId, string? cursor = null, int perPage = 500, CancellationToken cancellationToken = default);

        [Delete("/v4/accounts/{accountId}/rules/lists/{listId}/items")]
        Task<string> DeleteListItems(string accountId, string listId, [Body]DeleteListItemsRequest items, CancellationToken cancellationToken = default);

        [Post("/v4/accounts/{accountId}/rules/lists/{listId}/items")]
        Task<string> CreateListItems(string accountId, string listId, [Body] List<CreateListItem> request, CancellationToken cancellationToken = default);
    }
}
