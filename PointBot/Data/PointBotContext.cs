using Microsoft.EntityFrameworkCore;

using PointBot.Models;

namespace PointBot.Data
{
    public class PointBotContext : DbContext
    {
        public PointBotContext(DbContextOptions<PointBotContext> options) : base(options)
        {
        }
        
        public DbSet<PointEvent> PointEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PointEvent>()
                .Property(e => e.Created)
                .HasDefaultValueSql("now()");
        }
    }
}