namespace ACS.Shared.Configuration
{
    public class ServerConfiguration
    {
        public required TlsOptions Tls { get; set; }
    }

    public class TlsOptions
    {
        /// <summary>
        /// PKCS12 certificate file path
        /// </summary>
        public required string CertificatePath { get; set; }

        /// <summary>
        /// Private key passphrase
        /// </summary>
        public required string Password { get; set; }
    }
}
