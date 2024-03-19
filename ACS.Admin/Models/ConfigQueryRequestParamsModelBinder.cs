using ACS.Shared.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ACS.Admin.Models
{
    public class ConfigQueryRequestParamsModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string? agentName = bindingContext.ValueProvider.GetValue(nameof(ConfigQueryRequestParams.AgentName)).FirstValue;
            string? agentVersion = bindingContext.ValueProvider.GetValue(nameof(ConfigQueryRequestParams.AgentVersion)).FirstValue;

            if (string.IsNullOrEmpty(agentName) || string.IsNullOrEmpty(agentVersion))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            string? hostRoles = bindingContext.ValueProvider.GetValue(nameof(ConfigQueryRequestParams.HostRoles)).FirstValue;
            string? activeUsers = bindingContext.ValueProvider.GetValue(nameof(ConfigQueryRequestParams.ActiveUsers)).FirstValue;

            ConfigQueryRequestParams result = new ConfigQueryRequestParams
            {
                AgentName = agentName,
                AgentVersion = agentVersion,
                UserName = bindingContext.ValueProvider.GetValue(nameof(ConfigQueryRequestParams.UserName)).FirstValue,
                ActiveUsers = activeUsers?.Split(",", StringSplitOptions.TrimEntries),
                HostName = bindingContext.ValueProvider.GetValue(nameof(ConfigQueryRequestParams.HostName)).FirstValue,
                HostRoles = hostRoles?.Split(",", StringSplitOptions.TrimEntries),
                EnvironmentName = bindingContext.ValueProvider.GetValue(nameof(ConfigQueryRequestParams.EnvironmentName)).FirstValue
            };

            bindingContext.Result = ModelBindingResult.Success(result);

            return Task.CompletedTask;
        }
    }
}
