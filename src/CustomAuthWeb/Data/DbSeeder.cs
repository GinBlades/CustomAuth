using CustomAuthWeb.Models;
using CustomAuthWeb.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CustomAuthWeb.Data {
    public class DbSeeder {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _env;
        public DbSeeder(ApplicationDbContext db, IHostingEnvironment env) {
            _db = db;
            _env = env;
        }

        public async Task SeedAsync() {
            // For now, only run in development. Comment this out to set up initial administrator.
            if (!_env.IsDevelopment()) {
                return;
            }

            _db.Database.EnsureCreated();

            _db.Users.RemoveRange(await _db.Users.ToListAsync());

            var admin = new User() {
                Email = "admin@example.com",
                UserName = "Admin",
                Password = IdentityBasedHasher.HashPassword("secret").ToHashString(),
                Roles = UserRole.Administrator | UserRole.Member
            };

            var moderator = new User() {
                Email = "moderator@example.com",
                UserName = "Mod",
                Password = IdentityBasedHasher.HashPassword("secret").ToHashString(),
                Roles = UserRole.Moderator | UserRole.Member
            };

            var member = new User() {
                Email = "member@example.com",
                UserName = "Member",
                Password = IdentityBasedHasher.HashPassword("secret").ToHashString(),
                Roles = UserRole.Member
            };

            var guest = new User() {
                Email = "guest@example.com",
                UserName = "Guest",
                Password = IdentityBasedHasher.HashPassword("secret").ToHashString(),
                Roles = UserRole.Guest
            };
            var users = new User[] { admin, moderator, member, guest };
            await _db.Users.AddRangeAsync(users);
            await _db.SaveChangesAsync();
        }
    }
}
