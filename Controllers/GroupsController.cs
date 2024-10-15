using ChatApp.Hubs;
using ChatApp.Models;
using ChatAppp.Entity;
using ChatAppp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
            if (request == null || string.IsNullOrEmpty(request.GroupName) || request.Members == null || !request.Members.Any())
                return BadRequest("Invalid data.");

            // Validate CreatorId
            var creatorExists = await _context.Users.AnyAsync(u => u.Id == request.CreatorId);
            if (!creatorExists)
                return BadRequest("Invalid CreatorId.");

            // Validate that members exist
            var validMembers = await _context.Users
                .Where(u => request.Members.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            if (validMembers.Count != request.Members.Count)
                return BadRequest("One or more members do not exist.");

            string uniqueFileName = null;
            if (request.GroupImage != null && request.GroupImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/groups");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(request.GroupImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.GroupImage.CopyToAsync(fileStream);
                }

                request.ImageUrl = $"/images/groups/{uniqueFileName}";
            }

            var group = new Group
            {
                Name = request.GroupName,
                CreatorId = request.CreatorId,
                PhotoUrl = request.ImageUrl,
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                // Add members
                foreach (var memberId in validMembers)
                {
                    bool isAdmin = request.Admins?.Contains(memberId) ?? false;
                    _context.GroupMembers.Add(new GroupMember
                    {
                        GroupId = group.Id,
                        UserId = memberId,
                        IsAdmin = isAdmin
                    });

                    await _hubContext.Groups.AddToGroupAsync(memberId, group.Name);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _hubContext.Clients.Group(group.Name).SendAsync("GroupCreated", group.Name, request.Admins, validMembers);

                return Ok(new { GroupId = group.Id, GroupName = group.Name });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
