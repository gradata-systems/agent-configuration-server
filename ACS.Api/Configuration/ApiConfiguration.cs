using ACS.Shared.Configuration;

namespace ACS.Api.Configuration
{
    public class ApiConfiguration
    {
        public required ServerConfiguration Server { get; set; }

        public required ApiAuthConfiguration Authentication { get; set; }
    }
}
