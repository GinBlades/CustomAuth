using CustomAuthWeb.Models;
using CustomAuthWeb.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomAuthWeb.Data {
    public class DbSeeder {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _env;
        private readonly SimpleHasher _hasher;
        public DbSeeder(ApplicationDbContext db, IHostingEnvironment env, SimpleHasher hasher) {
            _db = db;
            _env = env;
            _hasher = hasher;
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
                Password = _hasher.HashWithEncryption("secret"),
                Roles = UserRole.Administrator | UserRole.Member
            };

            var moderator = new User() {
                Email = "moderator@example.com",
                UserName = "Mod",
                Password = _hasher.HashWithEncryption("secret"),
                Roles = UserRole.Moderator | UserRole.Member
            };

            var member = new User() {
                Email = "member@example.com",
                UserName = "Member",
                Password = _hasher.HashWithEncryption("secret"),
                Roles = UserRole.Member
            };

            var guest = new User() {
                Email = "guest@example.com",
                UserName = "Guest",
                Password = _hasher.HashWithEncryption("secret"),
                Roles = UserRole.Guest
            };
            var users = new User[] { admin, moderator, member, guest };
            await _db.Users.AddRangeAsync(users);
            await _db.SaveChangesAsync();
        }
    }
}
