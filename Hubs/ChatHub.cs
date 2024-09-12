using ChatApp.Services;
using ChatAppp.Entity;
using ChatAppp.Enum;
using ChatAppp.Models;
using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol.Plugins;

namespace ChatApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly GroupSrvice _groupSrvice;
        private readonly DBContext _dbContext;

        public ChatHub(GroupSrvice groupService, DBContext dbContext)
        {
            _groupSrvice = groupService;
            _dbContext = dbContext;
        }

        public async Task SendMessage(string receiverId, string messageContent, ChatAppp.Enum.MessageType messageType, string fileUrl = null)
        {
            var senderId = Context.UserIdentifier;

            var message = new ChatAppp.Models.Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageType = messageType,
                FileUrl = fileUrl,
                Content = messageContent,
                Timestamp = DateTime.Now
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            await Clients.Users(receiverId).SendAsync("ReceiveMessage", message);
        }

        public async Task SendMessageToGroup(string groupName, string messageContent, ChatAppp.Enum.MessageType messageType, string fileUrl = null)
        {
            var senderId = Context.UserIdentifier;

            var groupId = await _groupSrvice.GetGroupIdByName(groupName);

            if (groupId == 0)
            {
                throw new HubException("Group Not Found");
            }

            var message = new ChatAppp.Models.Message
            {
                SenderId = senderId,
                GroupId = groupId,
                MessageType = messageType,
                FileUrl = fileUrl,
                Content = messageContent,
                Timestamp = DateTime.Now
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }

        public Task JoinGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId,groupName);
        }

        public Task LeaveGroup(string groupName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId,groupName);
        }
    }
}
