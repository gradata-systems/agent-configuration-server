using ACS.Shared.Configuration;

namespace ACS.Api.Configuration
{
    public class ApiConfiguration
    {
        public required ServerConfiguration Server { get; set; }

        public required ApiAuthConfiguration Authentication { get; set; }

        /// <summary>
        /// How often to update the target / fragment cache
        /// </summary>
        public required int CacheUpdateIntervalMilliseconds { get; set; }
    }
}
