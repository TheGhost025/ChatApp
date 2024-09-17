using ChatApp.Hubs;
using ChatAppp.Models;
using ChatAppp.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Services;
using Microsoft.AspNetCore.Identity;
using ChatAppp.Entity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly GroupSrvice _groupService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DBContext _context;

        public ChatController(IHubContext<ChatHub> hubContext, GroupSrvice groupService , UserManager<ApplicationUser> userManager, DBContext dBContext)
        {
            _hubContext = hubContext;
            _groupService = groupService;
            _userManager = userManager;
            _context = dBContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string messageContent, MessageType messageType, string fileUrl = null)
        {
            await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", new Message
            {
                SenderId = User.Identity.Name,
                ReceiverId = receiverId,
                Content = messageContent,
                MessageType = messageType,
                FileUrl = fileUrl,
                Timestamp = DateTime.Now
            });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageToGroup(string groupName, string messageContent, MessageType messageType, string fileUrl = null)
        {
            // Fetch the Group ID using the GroupService
            int groupId = await _groupService.GetGroupIdByName(groupName);

            // Ensure group exists
            if (groupId == 0)
            {
                return NotFound("Group not found.");
            }

            // Send the message to the group
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", new Message
            {
                SenderId = User.Identity.Name,
                GroupId = groupId,
                Content = messageContent,
                MessageType = messageType,
                FileUrl = fileUrl,
                Timestamp = DateTime.Now
            });

            return RedirectToAction("Index");
        }


        // Search for new users not in the current user's friend list
        [HttpPost]
        public async Task<IActionResult> SearchNewUsers([FromBody] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term is required.");
            }
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Get users that are not friends of the current user
            var foundUsers = await _context.Users
                .Where(u => u.Id != currentUserId &&
                            (u.FirstName.Contains(searchTerm.ToLower()) || u.LastName.Contains(searchTerm.ToLower())) &&
                            !_context.FriendRequests.Any(fr => (fr.SenderId == currentUserId && fr.ReceiverId == u.Id) ||
                                                               (fr.ReceiverId == currentUserId && fr.SenderId == u.Id)))
                .ToListAsync();

            return Ok(foundUsers);
        }

        public async Task<IActionResult> SendFriendRequest([FromBody] string receiverId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (await _context.Users.AnyAsync(u => u.Id == receiverId) && currentUserId != receiverId)
            {
                // Ensure the friend request doesn't already exist
                var existingRequest = await _context.FriendRequests.AnyAsync(fr =>
                    (fr.SenderId == currentUserId && fr.ReceiverId == receiverId) ||
                    (fr.SenderId == receiverId && fr.ReceiverId == currentUserId));

                if (!existingRequest) 
                {
                    var friendRequest = new FriendRequest
                    {
                        SenderId = currentUserId,
                        ReceiverId = receiverId,
                        IsAccepted = false,
                        RequestDate = DateTime.Now
                    };

                    _context.FriendRequests.Add(friendRequest);
                    await _context.SaveChangesAsync();

                    return Ok("Friend request sent.");
                }
                else
                {
                    return BadRequest("Friend request already exists.");
                }
            }
            else
            {
                return BadRequest("Unable to send the friend request.");
            }
        }


    }
}
