using ChatAppp.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAppp.Models
{
    public class Message
    {
        public int Id { get; set; }
        [ForeignKey("Sender")]
        public string SenderId { get; set; }
        [ForeignKey("Receiver")]
        public string ReceiverId { get; set; }
        [ForeignKey("Group")]
        public int? GroupId { get; set; } // Nullable in case the message is not for a group
        public MessageType MessageType { get; set; }
        public string Content { get; set; }
        public string? FileUrl { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation properties
        public ApplicationUser Sender { get; set; }
        public ApplicationUser Receiver { get; set; }
        public Group Group { get; set; }
    }
}
