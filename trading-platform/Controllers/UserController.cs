using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trading_platform.Data;
using trading_platform.Models.Entities;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly TradingDbContext _context;

        public UserController(TradingDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _context.Users.ToList();
            return Ok(users);
        }
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        [HttpPost]
        public IActionResult CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, User updated)
        {
            var user = _context.Users.Find(id);
            if(user==null)
            {
                return NotFound();
            }
            user.Username = updated.Username;
            user.Email = updated.Email;
            user.PasswordHash = updated.PasswordHash;
            _context.SaveChanges();
            return Ok(user);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if(user==null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
