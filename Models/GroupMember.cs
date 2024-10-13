using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAppp.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        [ForeignKey("Group")]
        public int GroupId { get; set; }
        public bool IsAdmin { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Group Group { get; set; }
    }
}
