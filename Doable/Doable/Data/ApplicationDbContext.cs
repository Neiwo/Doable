using Microsoft.EntityFrameworkCore;
using Doable.Models;

namespace Doable.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Docu> Docus { get; set; }
        public DbSet<Tasklist> Tasklists { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Notes> Notes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tasklist>()
                .HasMany(t => t.Notes)
                .WithOne(n => n.Tasklist)
                .HasForeignKey(n => n.TaskID);

            modelBuilder.Entity<Tasklist>()
                .HasMany(t => t.Members)
                .WithOne(m => m.Tasklist)
                .HasForeignKey(m => m.TasklistID);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
