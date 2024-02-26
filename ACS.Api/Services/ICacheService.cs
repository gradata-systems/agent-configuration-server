using ACS.Shared.Models;

namespace ACS.Api.Services
{
    public interface ICacheService
    {
        bool TryGet(string agentName, out List<CacheEntry>? entries);
    }

    public class CacheEntry
    {
        public required Target Target { get; set; }

        public required string FragmentId { get; set; }

        public required string FragmentValue { get; set; }
    }
}
