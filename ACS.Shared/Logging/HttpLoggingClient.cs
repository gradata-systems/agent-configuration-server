using ACS.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Http;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ACS.Shared.Logging
{
    public class HttpLoggingClient : IHttpClient
    {
        private readonly HttpClient _httpClient;

        public HttpLoggingClient()
        {
            HttpClientHandler handler = new()
            {
                // Do not perform certificate validation for HTTP log dispatch
                ServerCertificateCustomValidationCallback = (HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors) => true
            };

            _httpClient = new(handler);
        }

        public void Configure(IConfiguration configuration)
        {
            HttpLoggingConfiguration? httpConfig = configuration.GetRequiredSection("Logging").Get<LoggingConfiguration>()?.Http;

            // Add configured headers to the logging request
            if (httpConfig?.Headers != null)
            {
                foreach (var header in httpConfig.Headers)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream, CancellationToken cancellationToken)
        {
            try
            {
                using StreamContent content = new(contentStream);
                content.Headers.Add("Content-Type", "application/json");

                using HttpResponseMessage result = await _httpClient
                    .PostAsync(requestUri, content, cancellationToken)
                    .ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send logs to: {Url}", requestUri);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
