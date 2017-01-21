using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CustomAuthWeb.Models {
    public class User {
        [Key, Required]
        public int Id { get; set; }
        [Required, EmailAddress, MaxLength(60)]
        public string Email { get; set; }
        [Required, MaxLength(30)]
        public string UserName { get; set; }
        // 102 Ensures the length is a hashed password in the current format.
        // If the password format changes, this will have to change also.
        [Required, StringLength(102, MinimumLength = 102)]
        public string Password { get; set; }
        [Required]
        public UserRole Roles { get; set; }
    }

    [Flags]
    public enum UserRole {
        Guest = 1 << 0, // Unpaid
        Member = 1 << 1, // Paid
        Moderator = 1 << 2, // Allowed edit rights to some resources
        Administrator = 1 << 3 // Full administration rights
    }
}
