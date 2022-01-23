using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnJsonWebToken.Shared.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }        
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = new byte[] { };
        public byte[] PasswordSalt { get; set; } = new byte[] { };
    }
}
