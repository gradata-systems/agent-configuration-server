using ACS.Shared.Providers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ACS.Shared.Models
{
    [Microsoft.EntityFrameworkCore.Index(nameof(Enabled))]
    public class Target
    {
        [ScaffoldColumn(false)]
        [ValidateNever]
        public int Id { get; set; }

        public string? Description { get; set; }

        [DisplayName("Agent Name")]
        [HelpText("Well-known agent name.")]
        [StringLength(32)]
        public required string AgentName { get; set; }

        [DisplayName("Min Version")]
        [HelpText("Minimum agent version to target.")]
        [RegularExpression(@"^[\d\.]+$")]
        [StringLength(16)]
        public string? AgentMinVersion { get; set; }

        [DisplayName("Max Version")]
        [HelpText("Maximum agent version to target.")]
        [RegularExpression(@"^[\d\.]+$")]
        [StringLength(16)]
        public string? AgentMaxVersion { get; set; }

        [DisplayName("User Name Pattern")]
        [HelpText("Regular expression to match against the current logged-on user (the user account the agent is running under).")]
        public string? UserNamePattern { get; set; }

        [DisplayName("Active User Name Pattern")]
        [HelpText("Regular expression to match against any currently logged-on users on the host.")]
        public string? ActiveUserNamePattern { get; set; }

        [DisplayName("Host Name Pattern")]
        [HelpText("Regular expression to match against the lower-case host name of the local machine.")]
        public string? HostNamePattern { get; set; }

        [DisplayName("Host Role Pattern")]
        [HelpText("Regular expression to match against each of the installed Windows server roles (case sensitive).")]
        public string? HostRolePattern { get; set; }

        [DisplayName("Environment Name Pattern")]
        [HelpText("Regular expression to match against the configured environment name (e.g. Production).")]
        public string? EnvironmentNamePattern { get; set; }

        public bool Enabled { get; set; }

        [ValidateNever]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public required DateTime Created { get; set; }

        [DisplayName("Created By")]
        [ValidateNever]
        [StringLength(256)]
        public required string CreatedBy { get; set; }

        [ValidateNever]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public required DateTime Modified { get; set; }

        [DisplayName("Modified By")]
        [ValidateNever]
        [StringLength(256)]
        public required string ModifiedBy { get; set; }

        /// <summary>
        /// Number of fragments linked to this target
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public int? LinkedFragments { get; set; }

        /// <summary>
        /// IDs of fragments linked to this target
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public List<int>? LinkedFragmentIds { get; set; }

        [ScaffoldColumn(false)]
        [ValidateNever]
        [JsonIgnore]
        public ICollection<TargetFragment>? TargetFragments { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
