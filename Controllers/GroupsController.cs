using ChatApp.Hubs;
using ChatApp.Models;
using ChatAppp.Entity;
using ChatAppp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GroupsController(DBContext context, IHubContext<ChatHub> hubContext, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromForm] CreateGroupRequest request)
        {
            string uniqueFileName = null;

            // Check if the group image is provided and if it has a file length greater than 0
            if (request.GroupImage != null && request.GroupImage.Length > 0)
            {
                // Define the folder where images will be stored
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/groups");
                // Generate a unique filename for the image
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(request.GroupImage.FileName);
                // Combine the uploads folder path with the unique filename
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Ensure the uploads folder exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Copy the file to the server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.GroupImage.CopyToAsync(fileStream);
                }

                // Set the image URL for saving in the database
                request.ImageUrl = $"/images/groups/{uniqueFileName}";
            }
            if (request == null || string.IsNullOrEmpty(request.GroupName) || request.Members == null || request.Admins == null)
                return BadRequest("Invalid data.");

            // Create the group
            var group = new Group
            {
                Name = request.GroupName,
                CreatorId = request.CreatorId,
                PhotoUrl = request.ImageUrl,
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Add members to the group and set admins
            foreach (var memberId in request.Members)
            {
                bool isAdmin = request.Admins.Contains(memberId); // Check if this member is an admin
                _context.GroupMembers.Add(new GroupMember
                {
                    GroupId = group.Id,
                    UserId = memberId,
                    IsAdmin = isAdmin
                });

                // Add the member to the SignalR group for real-time communication
                await _hubContext.Groups.AddToGroupAsync(memberId, group.Name);
            }
            await _context.SaveChangesAsync();

            // Notify all members of the new group and admins
            await _hubContext.Clients.Group(group.Name).SendAsync("GroupCreated", group.Name, request.Admins, request.Members);

            return Ok(new { GroupId = group.Id, GroupName = group.Name });
        }
    }
}
