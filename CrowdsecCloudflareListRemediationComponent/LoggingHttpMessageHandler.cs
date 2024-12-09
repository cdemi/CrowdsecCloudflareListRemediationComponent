using System.Diagnostics;

namespace CrowdsecCloudflareListRemediationComponent
{
    public class LoggingHttpMessageHandler(
        ILogger<LoggingHttpMessageHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestInfo = new
            {
                Method = request.Method,
                Url = request.RequestUri,
                Headers = request.Headers,
                Body = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null
            };

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();
                var responseInfo = new
                {
                    StatusCode = (int)response.StatusCode,
                    Headers = response.Headers,
                    Body = response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken) : null,
                    Duration = stopwatch.Elapsed
                };

                logger.LogInformation(
                    "HTTP Request:\n" +
                    "Method: {Method}\n" +
                    "Url: {Url}\n" +
                    //"Headers: {Headers}\n" +
                    "Body: {Body}\n" +
                    "\nHTTP Response:\n" +
                    "StatusCode: {StatusCode}\n" +
                    //"Response Headers: {ResponseHeaders}\n" +
                    "Response Body: {ResponseBody}\n" +
                    "Duration: {Duration}",
                    requestInfo.Method,
                    requestInfo.Url,
                    //JsonSerializer.Serialize(requestInfo.Headers),
                    requestInfo.Body,
                    responseInfo.StatusCode,
                    //JsonSerializer.Serialize(responseInfo.Headers),
                    responseInfo.Body,
                    responseInfo.Duration);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var responseInfo = new
                {
                    Duration = stopwatch.Elapsed
                };

                logger.LogError(ex,
                    "Exception during HTTP Request:\n" +
                    "Method: {Method}\n" +
                    "Url: {Url}\n" +
                    //"Headers: {Headers}\n" +
                    "Body: {Body}\n" +
                    "Duration: {Duration}",
                    requestInfo.Method,
                    requestInfo.Url,
                    //JsonSerializer.Serialize(requestInfo.Headers),
                    requestInfo.Body,
                    responseInfo.Duration);

                throw;
            }
        }
    }
}
