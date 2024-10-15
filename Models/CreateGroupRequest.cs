namespace ChatApp.Models
{
    public class CreateGroupRequest
    {
        public string GroupName { get; set; }
        public string CreatorId { get; set; }
        public List<string> Members { get; set; }
        public List<string> Admins { get; set; } // List of admin user IDs
        public IFormFile GroupImage { get; set; } // Image upload
        public string? ImageUrl { get; set; } // Path to store in DB
    }
}
