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
    }
}
