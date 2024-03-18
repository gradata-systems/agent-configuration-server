using ACS.Shared.Models;

namespace ACS.Shared.Services
{
    public interface ITargetMatchingService
    {
        bool IsMatch(Target target, ConfigQueryRequestParams requestParams);

        ConfigQueryResponse GetMatchingFragments(List<CacheEntry>? entries, ConfigQueryRequestParams requestParams);
    }
}
