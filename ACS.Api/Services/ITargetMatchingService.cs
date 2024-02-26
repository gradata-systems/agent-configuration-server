using ACS.Api.Models;
using ACS.Shared.Models;

namespace ACS.Api.Services
{
    public interface ITargetMatchingService
    {
        bool IsMatch(Target target, ConfigQueryRequestParams requestParams);
    }
}
