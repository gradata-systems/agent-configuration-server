using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ACS.Shared.Models
{
    [Microsoft.EntityFrameworkCore.Index(nameof(Enabled))]
    public class Fragment
    {
        [RegularExpression(@"^[a-zA-Z0-9\-_]+$")]
        [StringLength(32)]
        public required string Id { get; set; }

        public string? Description { get; set; }

        public required string Value { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [ValidateNever]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime Created { get; set; }

        [ValidateNever]
        [StringLength(256)]
        public required string CreatedBy { get; set; }

        [ValidateNever]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime Modified { get; set; }

        [ValidateNever]
        [StringLength(256)]
        public required string ModifiedBy { get; set; }

        /// <summary>
        /// Number of targets linked to this fragment
        /// </summary>
        [NotMapped]
        public required int LinkedTargets { get; set; }

        /// <summary>
        /// IDs of targets linked to this fragment
        /// </summary>
        [NotMapped]
        public List<int>? LinkedTargetIds { get; set; }

        [ScaffoldColumn(false)]
        [ValidateNever]
        public required ICollection<TargetFragment> TargetFragments { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
