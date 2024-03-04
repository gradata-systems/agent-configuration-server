using ACS.Shared.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace ACS.Shared.Utilities
{
    public static class TlsUtils
    {
        /// <summary>
        /// Imports a server certificate chain and private key from PEM
        /// </summary>
        /// <returns>PKCS12 certificate, suitable for use with Kestrel HTTPS</returns>
        public static X509Certificate2Collection LoadServerCertificateFromPEM(TlsOptions tlsOptions)
        {
            string keyContent = File.ReadAllText(tlsOptions.KeyPath);
            RSA key = RSA.Create();

            if (string.IsNullOrEmpty(tlsOptions.Password))
            {
                key.ImportFromPem(keyContent);
            }
            else
            {
                key.ImportFromEncryptedPem(keyContent, tlsOptions.Password);
            }

            X509Certificate2Collection chain = [];
            chain.ImportFromPemFile(tlsOptions.CertificatePath);

            chain[0] = chain[0].CopyWithPrivateKey(key);

            return chain;
        }
    }
}
