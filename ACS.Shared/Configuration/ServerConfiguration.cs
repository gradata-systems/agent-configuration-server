namespace ACS.Shared.Configuration
{
    public class ServerConfiguration
    {
        public int Port { get; set; }

        public required TlsOptions Tls { get; set; }
    }

    public class TlsOptions
    {
        /// <summary>
        /// PEM encrypted certificate file path
        /// </summary>
        public required string CertificatePath { get; set; }

        /// <summary>
        /// PEM private key file path
        /// </summary>
        public required string KeyPath { get; set; }

        /// <summary>
        /// Private key passphrase
        /// </summary>
        public required string Password { get; set; }
    }
}
