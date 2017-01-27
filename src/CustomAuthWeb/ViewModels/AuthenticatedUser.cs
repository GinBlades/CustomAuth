using CustomAuthWeb.Models;

namespace CustomAuthWeb.ViewModels {
    public class AuthenticatedUser {

        public AuthenticatedUser(User user) {
            Id = user.Id;
            Email = user.Email;
            UserName = user.UserName;
            Roles = user.Roles;
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public UserRole Roles { get; set; }
    }
}
