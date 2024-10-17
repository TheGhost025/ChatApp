using ChatApp.Services;
using ChatAppp.Entity;
using ChatAppp.Enum;
using ChatAppp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
            try
            {
                var senderId = Context.UserIdentifier;

                if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
                {
                    throw new HubException("Invalid sender or receiver ID.");
                }

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
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in SendMessage: {ex.Message}");
                throw;
            }
        }


        public async Task SendMessageToGroup(string groupName, string messageContent, ChatAppp.Enum.MessageType messageType, string fileUrl = null)
        {
            var senderId = Context.UserIdentifier;

            // Retrieve the group ID by group name
            var groupId = await _groupSrvice.GetGroupIdByName(groupName);

            if (groupId == 0)
            {
                throw new HubException("Group Not Found");
            }

            // Retrieve the sender details (name and photo)
            var sender = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == senderId);
            if (sender == null)
            {
                throw new HubException("Sender not found");
            }

            // Create the message and save it to the database
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

            // Prepare the data to be sent to the clients, including sender's name and photo
            var messageData = new
            {
                SenderId = senderId,
                SenderName = $"{sender.FirstName} {sender.LastName}",  // Concatenate sender's first and last name
                SenderPhotoUrl = sender.PhotoName,  // Assuming this is the path to the sender's profile photo
                Content = messageContent,
                MessageType = messageType,
                FileUrl = fileUrl,
                Timestamp = message.Timestamp
            };

            // Broadcast the message to the group along with the sender's name and photo
            await Clients.Group(groupName).SendAsync("ReceiveMessage", messageData);
        }


        // Method to notify the receiver about the friend request
        public async Task SendFriendRequestNotification(string receiverId, string senderName)
        {
            await Clients.User(receiverId).SendAsync("ReceiveFriendRequest", senderName);
        }

        public async Task CreateGroup(string groupName, string admin, List<string> members)
        {
            // Create the group in the database (not shown)
            // Assign the creator as the admin

            // Add users to the group
            foreach (var member in members)
            {
                await Groups.AddToGroupAsync(member, groupName);
            }

            // Notify group members
            await Clients.Group(groupName).SendAsync("GroupCreated", groupName, admin, members);
        }

        public Task JoinGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId,groupName);
        }

        public Task LeaveGroup(string groupName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId,groupName);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            // Notify other users that this user is online
            await Clients.Others.SendAsync("UserOnline", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.UserIdentifier;

            // Notify other users that this user is offline
            await Clients.Others.SendAsync("UserOffline", userId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
