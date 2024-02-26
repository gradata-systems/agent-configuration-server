using ACS.Api.Configuration;
using ACS.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace ACS.Api.Services
{
    public class CacheService : ICacheService, IDisposable
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        
        private Dictionary<string, List<CacheEntry>>? _cache = null;
        private readonly System.Timers.Timer _updateTimer;
        private readonly object _lock = new();

        public CacheService(IDbContextFactory<AppDbContext> dbContextFactory, IOptions<ApiConfiguration> config)
        {
            _dbContextFactory = dbContextFactory;

            _updateTimer = new System.Timers.Timer
            {
                Interval = config.Value.CacheUpdateIntervalMilliseconds,
                AutoReset = true,
                Enabled = true
            };

            _updateTimer.Elapsed += (sender, args) => UpdateCache();
        }

        public bool TryGet(string agentName, out List<CacheEntry>? cacheEntry)
        {
            lock (_lock)
            {
                if (_cache == null)
                {
                    UpdateCache();
                }

                return _cache!.TryGetValue(agentName, out cacheEntry);
            }
        }

        /// <summary>
        /// Populates the cache with a flattened map of targets to fragments, keyed by the agent name.
        /// </summary>
        private void UpdateCache()
        {
            lock (_lock)
            {
                using (AppDbContext dbContext = _dbContextFactory.CreateDbContext())
                {
                    _cache = (
                        from tf in dbContext.TargetFragments
                        join t in dbContext.Targets on tf.TargetId equals t.Id
                        join f in dbContext.Fragments on tf.FragmentId equals f.Id
                        where t.Enabled == true && f.Enabled == true
                        select new CacheEntry
                        {
                            Target = t,
                            FragmentId = f.Id,
                            FragmentValue = f.Value
                        }
                    )
                    .GroupBy(item => item.Target.AgentName)
                    .ToDictionary(item => item.Key, item => item.ToList());

                    Log.Debug("Filled target cache with {KeyCount} keys", _cache.Count);
                }
            }
        }

        public void Dispose()
        {
            _updateTimer.Dispose();
        }
    }
}
