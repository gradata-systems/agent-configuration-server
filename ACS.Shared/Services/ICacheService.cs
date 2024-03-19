using ACS.Shared.Models;

namespace ACS.Shared.Services
{
    public interface ICacheService
    {
        bool TryGet(string agentName, out List<CompiledCacheEntry>? entries);

        Task<List<CompiledCacheEntry>?> GetAsync(string agentName);
    }

    public class CacheEntry
    {
        public required Target Target { get; set; }

        public required Fragment Fragment { get; set; }
    }

    public class CompiledCacheEntry
    {
        public CompiledTarget Target { get; set; }

        public Fragment Fragment { get; set; }

        public CompiledCacheEntry(CacheEntry entry)
        {
            Target = new CompiledTarget(entry.Target);
            Fragment = entry.Fragment;
        }
    }
}
