using ChatAppp.Entity;
using ChatAppp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public class UserService
    {
        private readonly DBContext _context;  // Ensure DBContext is correct

        public UserService(DBContext context)
        {
            _context = context;  // Constructor injection
        }

        public async Task<Message[]> GetPrivateChatIdByName(string senderId,string reciverId)
        {
            var chats = await _context.Messages.Where(c => (c.SenderId == senderId && c.ReceiverId == reciverId) || (c.SenderId == reciverId && c.ReceiverId == senderId)).ToArrayAsync();

            return chats;
        }
    }
}
