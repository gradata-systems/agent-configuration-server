using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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
        [StringLength(32)]
        public required string AgentName { get; set; }

        [DisplayName("Agent Min Version")]
        [RegularExpression(@"^[\d\.]+$")]
        [StringLength(16)]
        public string? AgentMinVersion { get; set; }

        [DisplayName("Agent Max Version")]
        [RegularExpression(@"^[\d\.]+$")]
        [StringLength(16)]
        public string? AgentMaxVersion { get; set; }

        [DisplayName("User Name Pattern")]
        public string? UserNamePattern { get; set; }

        [DisplayName("Host Name Pattern")]
        public string? HostNamePattern { get; set; }

        public bool Enabled { get; set; }

        [ValidateNever]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public required DateTime Created { get; set; }

        [ValidateNever]
        [StringLength(256)]
        public required string CreatedBy { get; set; }

        [ValidateNever]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public required DateTime Modified { get; set; }

        [ValidateNever]
        [StringLength(256)]
        public required string ModifiedBy { get; set; }

        /// <summary>
        /// Number of fragments linked to this target
        /// </summary>
        [NotMapped]
        public int LinkedFragments { get; set; }

        /// <summary>
        /// IDs of fragments linked to this target
        /// </summary>
        [NotMapped]
        public List<string>? LinkedFragmentIds { get; set; }

        [ScaffoldColumn(false)]
        [ValidateNever]
        public required ICollection<TargetFragment> TargetFragments { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
