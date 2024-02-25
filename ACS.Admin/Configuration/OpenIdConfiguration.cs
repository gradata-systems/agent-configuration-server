namespace ACS.Admin.Configuration
{
    public class OpenIdConfiguration
    {
        public required string AuthorityUrl { get; set; }

        public required string ClientId { get; set; }

        public required string ClientSecret { get; set; }

        public required IEnumerable<string> Scopes { get; set; }
    }
}
