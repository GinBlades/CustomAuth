using System.ComponentModel.DataAnnotations;

namespace CustomAuthWeb.FormObjects.Account {
    public class LoginFormObject {
        [EmailAddress, MaxLength(60)]
        public string Email { get; set; }
        [MaxLength(30)]
        public string UserName { get; set; }
        [Required, MaxLength(30)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        [MaxLength(120)]
        public string ReturnUrl { get; set; }
    }
}
