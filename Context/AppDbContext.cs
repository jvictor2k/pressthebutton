using Microsoft.EntityFrameworkCore;
using PressTheButton.Models;

namespace PressTheButton.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Question> Questions { get; set; }
        public DbSet<UserResponse> UserResponses { get; set; }
    }
}
