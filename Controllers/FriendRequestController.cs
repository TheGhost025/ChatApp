using ChatApp.Hubs;
using ChatAppp.Entity;
using ChatAppp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    public class FriendRequestController : Controller
    {
        private readonly DBContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public FriendRequestController(DBContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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

                    var sender = _context.Users.FirstOrDefault(u => u.Id == currentUserId);

                    // Notify the receiver in real-time using SignalR
                    await _hubContext.Clients.User(receiverId).SendAsync("ReceiveFriendRequest", new
                    {
                        FriendRequestId = friendRequest.Id,
                        SenderId = currentUserId,
                        SenderFirstName = sender.FirstName,  // First name
                        SenderLastName = sender.LastName,    // Last name
                        SenderImage = sender.PhotoName,       // Profile image URL
                        RequestDate = DateTime.Now,
                    });

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

        [HttpGet("GetPendingFriendRequests")]
        public async Task<IActionResult> GetPendingFriendRequests()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == null)
            {
                return Unauthorized();
            }

            // Retrieve pending friend requests where the current user is the receiver
            var pendingRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == currentUserId && !fr.IsAccepted)
                .Select(fr => new
                {
                    fr.Id,
                    fr.SenderId,
                    SenderFirstName = fr.Sender.FirstName,   // Fetch first name from the sender (ApplicationUser)
                    SenderLastName = fr.Sender.LastName,     // Fetch last name from the sender (ApplicationUser)
                    SenderPhoto = fr.Sender.PhotoName,       // Fetch profile photo from the sender (ApplicationUser)
                    fr.RequestDate
                })
                .ToListAsync();

            return Ok(pendingRequests);
        }

        [HttpGet("GetPendingFriendRequestsSender")]
        public async Task<IActionResult> GetPendingFriendRequestsSender()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == null)
            {
                return Unauthorized();
            }

            // Retrieve pending friend requests where the current user is the receiver
            var pendingRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == currentUserId && !fr.IsAccepted)
                .Select(fr => new
                {
                    fr.Id,
                    fr.SenderId,
                    SenderFirstName = fr.Sender.FirstName,   // Fetch first name from the sender (ApplicationUser)
                    SenderLastName = fr.Sender.LastName,     // Fetch last name from the sender (ApplicationUser)
                    SenderPhoto = fr.Sender.PhotoName,       // Fetch profile photo from the sender (ApplicationUser)
                    fr.RequestDate
                })
                .ToListAsync();

            return Ok(pendingRequests);
        }


        [HttpPost]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] int requestId)
        {
            var friendRequest = await _context.FriendRequests.FindAsync(requestId);

            if (friendRequest == null)
            {
                return NotFound("Friend request not found.");
            }

            friendRequest.IsAccepted = true;
            await _context.SaveChangesAsync();

            return Ok("Friend request accepted.");
        }

        [HttpPost]
        public async Task<IActionResult> DeclineFriendRequest([FromBody] int requestId)
        {
            var friendRequest = await _context.FriendRequests.FindAsync(requestId);

            if (friendRequest == null)
            {
                return NotFound("Friend request not found.");
            }

            _context.FriendRequests.Remove(friendRequest);
            await _context.SaveChangesAsync();

            return Ok("Friend request declined.");
        }

        [HttpGet("GetSentFriendRequests")]
        public async Task<IActionResult> GetSentFriendRequests()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == null)
            {
                return Unauthorized();
            }

            // Retrieve sent friend requests where the current user is the sender and the requests are not accepted yet
            var sentRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == currentUserId && !fr.IsAccepted)
                .Select(fr => new
                {
                    fr.Id,
                    fr.ReceiverId,
                    ReceiverFirstName = fr.Receiver.FirstName,   // Fetch first name from the receiver (ApplicationUser)
                    ReceiverLastName = fr.Receiver.LastName,     // Fetch last name from the receiver (ApplicationUser)
                    ReceiverPhoto = fr.Receiver.PhotoName,       // Fetch profile photo from the receiver (ApplicationUser)
                    fr.RequestDate
                })
                .ToListAsync();

            return Ok(sentRequests);
        }
    }
}
