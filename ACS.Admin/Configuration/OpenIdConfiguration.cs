namespace ACS.Admin.Configuration
{
    public class OpenIdConfiguration
    {
        public required string AuthorityUrl { get; set; }

        public required string ClientId { get; set; }

        public required string ClientSecret { get; set; }

        public required IEnumerable<string> Scopes { get; set; }

        /// <summary>
        /// Location of the CA certificate used to validate client certificates.
        /// If not specified, the system default certificate truststore is used.
        /// </summary>
        public required string CaTrustPath { get; set; }
    }
}
