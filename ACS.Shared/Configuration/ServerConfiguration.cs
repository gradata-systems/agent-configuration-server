namespace ACS.Shared.Configuration
{
    public class ServerConfiguration
    {
        public required TlsOptions Tls { get; set; }
    }

    public class TlsOptions
    {
        /// <summary>
        /// PEM certificate chain file path
        /// </summary>
        public required string CertificatePath { get; set; }

        /// <summary>
        /// Encrypted PEM key file path
        /// </summary>
        public required string KeyPath { get; set; }

        /// <summary>
        /// Private key passphrase
        /// </summary>
        public required string Password { get; set; }
    }
}
