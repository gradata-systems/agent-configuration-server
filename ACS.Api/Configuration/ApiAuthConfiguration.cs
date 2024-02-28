namespace ACS.Api.Configuration
{
    public class ApiAuthConfiguration
    {
        /// <summary>
        /// Location of the CA certificate used to validate client certificates.
        /// If not specified, the system default certificate truststore is used.
        /// </summary>
        public required string CaTrustPath { get; set; }

        /// <summary>
        /// Regex patterns used to match client certificate subject names.
        /// If none specified, any valid client certificate is allowed.
        /// </summary>
        public required List<string>? AuthorisedSubjects { get; set; }

        /// <summary>
        /// Name of the header containing the client certificate, where a reverse proxy is used.
        /// </summary>
        public string? ForwardedHeader { get; set; }
    }
}
