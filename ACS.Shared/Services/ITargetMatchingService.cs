using ACS.Shared.Models;

namespace ACS.Shared.Services
{
    public interface ITargetMatchingService
    {
        bool IsMatch(CompiledTarget target, ConfigQueryRequestParams requestParams);

        ConfigQueryResponse GetMatchingFragments(List<CompiledCacheEntry>? entries, ConfigQueryRequestParams requestParams);
    }
}
