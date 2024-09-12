using ChatAppp.Entity;

namespace ChatApp.Services
{
    public class GroupSrvice
    {
        private readonly DBContext _context;  // Ensure DBContext is correct

        public GroupSrvice(DBContext context)
        {
            _context = context;  // Constructor injection
        }

        public async Task<int> GetGroupIdByName(string groupName)
        {
            var group = _context.Groups.Where(g => g.Name == groupName).Select(g => g.Id).FirstOrDefault();

            return group;
        }
    }
}
