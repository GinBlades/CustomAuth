using System;
using System.ComponentModel.DataAnnotations;

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
        // Passed to filter
        GuestAccess = 1 << 0, // Unpaid
        MemberAccess = 1 << 1, // Paid
        ModeratorAccess = 1 << 2, // Allowed edit rights to some resources
        AuthorAccess = 1 << 3, // Allowed to edit only post resources (for example)
        AdministratorAccess = 1 << 4, // Full administration rights

        // Given to users
        Guest = GuestAccess,
        Member = Guest | MemberAccess,
        Moderator = Member | ModeratorAccess,
        Author = Member | AuthorAccess,
        Administrator = Moderator | AdministratorAccess
    }
}
