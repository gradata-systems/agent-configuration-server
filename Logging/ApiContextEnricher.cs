using AgentConfigurationServer.Models;
using Microsoft.Net.Http.Headers;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Net;
using System.Security.Principal;

namespace AgentConfigurationServer.Logging
{
    /// <summary>
    /// Custom Serilog enricher that adds user attribution and remote address properties to each log event where a HTTP context exists
    /// </summary>
    public class ApiContextEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiContextEnricher()
            : this(new HttpContextAccessor())
        { }

        public ApiContextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            HttpContext httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            string hostName = Dns.GetHostName();
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ServerHostName", hostName));
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ServerAddress", Dns.GetHostAddresses(hostName)));

            // Prefer the X-Forwarded-For header. If unset, fall back to the remote IP address
            string remoteIpAddress = httpContext.Request.Headers["X-Forwarded-For"].ToString();
            if (string.IsNullOrEmpty(remoteIpAddress))
            {
                remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RemoteAddress", remoteIpAddress.ToString()));

            string userAgent = httpContext.Request.Headers[HeaderNames.UserAgent];
            if (!string.IsNullOrEmpty(userAgent))
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("UserAgent", userAgent));
            }

            string referer = httpContext.Request.Headers[HeaderNames.Referer];
            if (!string.IsNullOrEmpty(referer))
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("Referer", referer));
            }

            IIdentity userIdentity = httpContext.User?.Identity;
            if (userIdentity != null)
            {
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("UserAuthenticated", userIdentity.IsAuthenticated));

                if (userIdentity.IsAuthenticated)
                {
                    ClaimsIdentity claimsIdentity = new(httpContext.User);
                    logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("UserId", claimsIdentity.Id));

                    if (!string.IsNullOrEmpty(claimsIdentity.Name))
                    {
                        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("UserName", claimsIdentity.Name));
                    }
                    if (!string.IsNullOrEmpty(claimsIdentity.Email))
                    {
                        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("UserEmail", claimsIdentity.Email));
                    }
                }
            }
        }
    }

    public static class ApiContextEnricherConfigurationExtensions
    {
        public static LoggerConfiguration WithApiContext(this LoggerEnrichmentConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            return config.With<ApiContextEnricher>();
        }
    }
}
