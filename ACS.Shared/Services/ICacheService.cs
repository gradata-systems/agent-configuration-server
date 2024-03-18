using ACS.Shared.Models;

namespace ACS.Shared.Services
{
    public interface ICacheService
    {
        bool TryGet(string agentName, out List<CacheEntry>? entries);

        Task<List<CacheEntry>?> GetAsync(string agentName);
    }

    public class CacheEntry
    {
        public required Target Target { get; set; }

        public required Fragment Fragment { get; set; }
    }
}
