using CustomAuthWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CustomAuthWeb.FormObjects.Account {
    public class RegisterFormObject {
        [Required, EmailAddress, MaxLength(60)]
        public string Email { get; set; }
        [Required, MaxLength(30)]
        public string UserName { get; set; }
        [Required, MaxLength(30)]
        public string Password { get; set; }
        [Required, MaxLength(30), Compare("Password")]
        public string PasswordConfirmation { get; set; }
        [MaxLength(120)]
        public string ReturnUrl { get; set; }

        public User ToUser() {
            return new User() {
                Email = Email,
                UserName = UserName,
                Password = Password,
                Roles = UserRole.Guest
            };
        }
    }
}
