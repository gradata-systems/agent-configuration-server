using System.ComponentModel.DataAnnotations;

namespace AgentConfigurationServer.Models
{
    public class TargetFragment
    {
        public int Id { get; set; }

        public int TargetId { get; set; }

        [StringLength(32)]
        public required string FragmentId { get; set; }

        public DateTimeOffset Created { get; set; }

        [StringLength(256)]
        public required string CreatedBy { get; set; }
    }
}
