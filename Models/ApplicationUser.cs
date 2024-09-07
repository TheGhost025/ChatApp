using Microsoft.AspNetCore.Identity;

namespace ChatAppp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhotoName { get; set; }
        public string Status { get; set; } = "Hi, I am using the chat app";
        public bool IsOnline { get; set; } = false;

        // Navigation properties
        public ICollection<FriendRequest> SentRequests { get; set; }
        public ICollection<FriendRequest> ReceivedRequests { get; set; }
        public ICollection<GroupMember> GroupMembers { get; set; }
        public ICollection<Message> SentMessages { get; set; }
        public ICollection<Message> ReceivedMessages { get; set; }
    }
}
