namespace ACS.Api.Configuration
{
    public class ApiAuthConfiguration
    {
        public required string CaTrustPath { get; set; }

        public required List<string> AuthorisedSubjects { get; set; }
    }
}
