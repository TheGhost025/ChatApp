using ChatAppp.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly DBContext _context;

        public SearchController(DBContext context) 
        {
            _context = context;
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
    }
}
