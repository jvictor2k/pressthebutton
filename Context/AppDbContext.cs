using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PressTheButton.Models;

namespace PressTheButton.Context
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Question> Questions { get; set; }

        public DbSet<UserResponse> UserResponses { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
