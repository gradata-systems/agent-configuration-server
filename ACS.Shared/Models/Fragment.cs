using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ACS.Shared.Models
{
    [Microsoft.EntityFrameworkCore.Index(nameof(Enabled))]
    public class Fragment
    {
        [ScaffoldColumn(false)]
        [ValidateNever]
        public int Id { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9\.\-_]+$")]
        [StringLength(128)]
        public required string Name { get; set; }

        /// <summary>
        /// Numeric priority that determines which fragment will be selected, where more than one targets match.
        /// A greater priority value means higher precedence. Fragments without a priority take less precedence than those with.
        /// </summary>
        public int? Priority { get; set; }

        public string? Description { get; set; }

        public string? Value { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

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
        /// Number of targets linked to this fragment
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public int? LinkedTargets { get; set; }

        /// <summary>
        /// IDs of targets linked to this fragment
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public List<int>? LinkedTargetIds { get; set; }

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
