using Azure.Security.KeyVault.Certificates;

namespace ACS.Admin.Auth
{
    public static class UserRole
    {
        public const string Administrator = "acs-admin";

        public const string ReadonlyUser = "acs-readonly-user";
    }
}
