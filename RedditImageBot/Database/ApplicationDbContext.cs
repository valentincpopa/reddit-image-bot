using Microsoft.EntityFrameworkCore;
using RedditImageBot.Database.Configurations;

namespace RedditImageBot.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new PostConfiguration());
            builder.ApplyConfiguration(new MessageConfiguration());
        }
    }
}
