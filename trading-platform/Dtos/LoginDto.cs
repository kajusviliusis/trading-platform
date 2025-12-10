using System.ComponentModel.DataAnnotations;

namespace trading_platform.Dtos
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password {  get; set; } = string.Empty;
    }
}
