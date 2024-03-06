using System.Security.Claims;

namespace ACS.Shared.Models
{
    /// <summary>
    /// Represents a claims identity and parses a Claim object
    /// </summary>
    public class ClaimsIdentity
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? GivenName { get; set; }

        public string? FamilyName { get; set; }

        public string? Email { get; set; }

        public List<string> Roles { get; set; }

        public ClaimsIdentity(ClaimsPrincipal principal)
        {
            foreach (var claim in principal.Claims)
            {
                switch (claim.Type)
                {
                    case ClaimTypes.NameIdentifier:
                        Id = claim.Value;
                        break;
                    case ClaimTypes.Name:
                    case "preferred_username":
                        Name = claim.Value;
                        break;
                    case ClaimTypes.GivenName:
                        GivenName = claim.Value;
                        break;
                    case ClaimTypes.Surname:
                        FamilyName = claim.Value;
                        break;
                    case ClaimTypes.Email:
                        Email = claim.Value;
                        break;
                }
            }

            Roles = principal.Claims
                .Where(claim => claim.Type == ClaimTypes.Role)
                .Select(claim => claim.Value)
                .ToList();
        }

        public static ClaimsIdentity FromPrincipal(ClaimsPrincipal principal)
        {
            return new ClaimsIdentity(principal);
        }
    }
}
