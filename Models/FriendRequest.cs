using ChatApp.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatAppp.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        [ForeignKey("Sender")]
        public string SenderId { get; set; }
        [ForeignKey("Receiver")]
        public string ReceiverId { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime RequestDate { get; set; }

        // Navigation properties
        public ApplicationUser Sender { get; set; }
        public ApplicationUser Receiver { get; set; }
    }
}
