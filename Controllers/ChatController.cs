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
        private readonly UserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DBContext _context;

        public ChatController(IHubContext<ChatHub> hubContext, GroupSrvice groupService , UserManager<ApplicationUser> userManager,UserService userService, DBContext dBContext)
        {
            _hubContext = hubContext;
            _groupService = groupService;
            _userManager = userManager;
            _context = dBContext;
            _userService = userService;
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

            // Fetch messages where the user is either sender, receiver, or part of a group
            var messages = _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId || m.Group.GroupMembers.Any(gm => gm.UserId == userId))
                .ToList();  // Materialize the result to work in memory

            var recentChats = messages
                .GroupBy(m => m.GroupId.HasValue
                            ? m.GroupId.ToString()  // Use GroupId for group chats
                            : (m.SenderId == userId ? m.ReceiverId : m.SenderId))  // Use the other user's ID for one-on-one chats
                .Select(group => {
                    var lastMessage = group.OrderByDescending(m => m.Timestamp).FirstOrDefault();

                    if (lastMessage == null)
                    {
                        return null; // Handle case where there is no message
                    }

                    var chatPartner = lastMessage.GroupId.HasValue
                        ? _context.Groups.FirstOrDefault(g => g.Id == lastMessage.GroupId).Name//lastMessage.Group.Name  // For group chats
                        : (lastMessage.SenderId == userId
                            ? _context.Users.FirstOrDefault(u => u.Id == lastMessage.ReceiverId).FirstName + " " + _context.Users.FirstOrDefault(u => u.Id == lastMessage.ReceiverId).LastName
                            : _context.Users.FirstOrDefault(u => u.Id == lastMessage.SenderId).FirstName + " " + _context.Users.FirstOrDefault(u => u.Id == lastMessage.SenderId).LastName);  // For direct messages, use null checks

                    var photoUrl = lastMessage.GroupId.HasValue 
                        ? _context.Groups.FirstOrDefault(g => g.Id == lastMessage.GroupId).PhotoUrl
                        : (lastMessage.SenderId == userId
                            ? _context.Users.FirstOrDefault(u => u.Id == lastMessage.ReceiverId).PhotoName
                            : _context.Users.FirstOrDefault(u => u.Id == lastMessage.SenderId).PhotoName); // Use null checks here as well

                    return new
                    {
                        ConversationId = lastMessage.GroupId.HasValue
                            ? lastMessage.GroupId.ToString()  // For group chats, use GroupId
                            : (lastMessage.SenderId == userId ? lastMessage.ReceiverId : lastMessage.SenderId), // For one-on-one chats
                        LastMessage = lastMessage.Content,
                        LastMessageTimestamp = lastMessage.Timestamp,
                        ChatPartner = chatPartner,
                        PhotoUrl = photoUrl
                    };

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

        [HttpGet("GetPrivateChat")]
        public async Task<IActionResult> GetUserChat(string reciverId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (reciverId == null)
            {
                return NotFound();
            }

            // Return the user's chat history, you might need to modify this depending on your data structure
            var chatHistory = await _userService.GetPrivateChatIdByName(userId,reciverId);
            return Ok(chatHistory);
        }


        [HttpGet("GetGroupMessages")]
        public IActionResult GetGroupMessages(int groupId)
        {
            var messages = _context.Messages
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    SenderId = m.SenderId,
                    Sender = m.Sender.FirstName + " " + m.Sender.LastName,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    SenderPhotoUrl = m.Sender.PhotoName // Include the sender's photo URL
                })
                .ToList();

            return Ok(messages);
        }

    }
}
