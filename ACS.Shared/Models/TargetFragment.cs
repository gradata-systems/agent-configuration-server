using System.ComponentModel.DataAnnotations;

namespace ACS.Shared.Models
{
    public class TargetFragment
    {
        public int Id { get; set; }

        public int TargetId { get; set; }

        public required int FragmentId { get; set; }

        public required DateTime Created { get; set; }

        [StringLength(256)]
        public required string CreatedBy { get; set; }
    }
}
