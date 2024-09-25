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
using NuGet.Protocol.Plugins;

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
        public async Task<IActionResult> SendMessage(string receiverId, string messageContent, ChatAppp.Enum.MessageType messageType, string fileUrl = null)
        {
            await _hubContext.Clients.User(receiverId).SendAsync("ReceiveMessage", new ChatAppp.Models.Message
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
        public async Task<IActionResult> SendMessageToGroup(string groupName, string messageContent, ChatAppp.Enum.MessageType messageType, string fileUrl = null)
        {
            // Fetch the Group ID using the GroupService
            int groupId = await _groupService.GetGroupIdByName(groupName);

            // Ensure group exists
            if (groupId == 0)
            {
                return NotFound("Group not found.");
            }

            // Send the message to the group
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveMessage", new ChatAppp.Models.Message
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

        [HttpGet("GetFriends")]
        public IActionResult GetFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the currently authenticated user

            var friends = _context.FriendRequests
                .Where(fr => (fr.SenderId == userId || fr.ReceiverId == userId) && fr.IsAccepted)
                .Select(fr => new {
                    FriendId = fr.SenderId == userId ? fr.Receiver.Id : fr.Sender.Id,
                    FriendName = fr.SenderId == userId ? fr.Receiver.FirstName + " " + fr.Receiver.LastName : fr.Sender.FirstName + " " + fr.Sender.LastName,
                    PhotoUrl = fr.SenderId == userId ? fr.Receiver.PhotoName : fr.Sender.PhotoName
                }).ToList();

            return Ok(friends);
        }


        [HttpGet("GetRecentChats")]
        public IActionResult GetRecentChats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get recent chats where the user is either sender or receiver
            var recentChats = _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId || m.Group.GroupMembers.Any(gm => gm.UserId == userId))
                .GroupBy(m => new { m.SenderId, m.ReceiverId, m.GroupId })  // Group by sender-receiver or group
                .Select(group => new {
                    ConversationId = group.Key.GroupId.ToString() ?? (group.Key.SenderId + "-" + group.Key.ReceiverId), // Use groupId or combine user ids
                    LastMessage = group.OrderByDescending(m => m.Timestamp).FirstOrDefault().Content,
                    LastMessageTimestamp = group.OrderByDescending(m => m.Timestamp).FirstOrDefault().Timestamp,
                    ChatPartner = group.Key.GroupId.HasValue ? group.FirstOrDefault().Group.Name :
                                  (group.Key.SenderId == userId ? group.FirstOrDefault().Receiver.FirstName + " " + group.FirstOrDefault().Receiver.LastName
                                                                 : group.FirstOrDefault().Sender.FirstName + " " + group.FirstOrDefault().Sender.LastName),
                    PhotoUrl = group.Key.GroupId.HasValue ? group.FirstOrDefault().Group.PhotoUrl :
                                (group.Key.SenderId == userId ? group.FirstOrDefault().Receiver.PhotoName : group.FirstOrDefault().Sender.PhotoName)
                })
                .OrderByDescending(c => c.LastMessageTimestamp)
                .ToList();

            return Ok(recentChats);
        }


        [HttpGet("GetGroups")]
        public IActionResult GetGroups()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var groups = _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Select(gm => new {
                    gm.Group.Id,
                    gm.Group.Name,
                    gm.Group.PhotoUrl,
                    MemberCount = gm.Group.GroupMembers.Count()
                }).ToList();

            return Ok(groups);
        }

    }
}
