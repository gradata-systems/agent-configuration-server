using ACS.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace ACS.Shared.Services
{
    public class CacheService : ICacheService, IDisposable
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        
        private Dictionary<string, List<CacheEntry>>? _cache = null;
        private readonly System.Timers.Timer _updateTimer;
        private readonly object _lock = new();

        public CacheService(IDbContextFactory<AppDbContext> dbContextFactory, IOptions<CacheConfiguration> config)
        {
            _dbContextFactory = dbContextFactory;

            _updateTimer = new System.Timers.Timer
            {
                Interval = config.Value.UpdateIntervalMilliseconds ?? int.MaxValue,
                Enabled = config.Value.UpdateIntervalMilliseconds.HasValue,
                AutoReset = true
            };

            _updateTimer.Elapsed += (sender, args) => UpdateCache();
        }

        public bool TryGet(string agentName, out List<CacheEntry>? cacheEntries)
        {
            lock (_lock)
            {
                if (_cache == null)
                {
                    UpdateCache();
                }

                return _cache!.TryGetValue(agentName, out cacheEntries);
            }
        }

        /// <summary>
        /// Lookup target fragments immediately, with no cache
        /// </summary>
        public async Task<List<CacheEntry>?> GetAsync(string agentName)
        {
            using AppDbContext dbContext = _dbContextFactory.CreateDbContext();
            Dictionary<string, List<CacheEntry>> entriesByAgent = await GetCacheEntries(dbContext).ToDictionaryAsync(item => item.Key, item => item.ToList());
            
            if (entriesByAgent!.TryGetValue(agentName, out List<CacheEntry>? entries))
            {
                return entries;
            }

            return null;
        }

        /// <summary>
        /// Populates the cache with a flattened map of targets to fragments, keyed by the agent name.
        /// </summary>
        private void UpdateCache()
        {
            lock (_lock)
            {
                using AppDbContext dbContext = _dbContextFactory.CreateDbContext();
                _cache = GetCacheEntries(dbContext).ToDictionary(item => item.Key, item => item.ToList());

                Log.Debug("Filled target cache with {KeyCount} keys", _cache.Count);
            }
        }

        private static IQueryable<IGrouping<string, CacheEntry>> GetCacheEntries(AppDbContext dbContext)
        {
            return (from tf in dbContext.TargetFragments
                    join t in dbContext.Targets on tf.TargetId equals t.Id
                    join f in dbContext.Fragments on tf.FragmentId equals f.Id
                    where t.Enabled == true && f.Enabled == true
                    select new CacheEntry
                    {
                        Target = t,
                        Fragment = f
                    })
                    .GroupBy(item => item.Target.AgentName);
        }

        public void Dispose()
        {
            _updateTimer.Dispose();
        }
    }
}
