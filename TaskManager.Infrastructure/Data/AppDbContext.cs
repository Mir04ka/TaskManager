using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Process> Processes => Set<Process>();
    public DbSet<ProcessUser> ProcessUsers => Set<ProcessUser>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();
    public DbSet<TaskItemTag> TaskItemTags => Set<TaskItemTag>();
    public DbSet<TaskRemark> TaskRemarks => Set<TaskRemark>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Username).HasMaxLength(100);
            e.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<Process>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasMany(x => x.Users)
                .WithOne()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProcessUser>(e =>
        {
            e.HasKey(x => new { x.ProcessId, x.UserId });
            e.Property(x => x.Role).HasConversion<string>();
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => x.ProcessId);
            e.HasIndex(x => x.AssignedToUserId);
            e.HasOne(x => x.Process)
                .WithMany()
                .HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.AssignedToUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskTag>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100);
            e.HasIndex(x => x.ProcessId);
        });

        modelBuilder.Entity<TaskItemTag>(e =>
        {
            e.HasKey(x => new { x.TaskId, x.TagId });
            e.HasOne(x => x.Task)
                .WithMany(t => t.Tags)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Tag)
                .WithMany()
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskRemark>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).HasMaxLength(2000);
            e.HasIndex(x => x.TaskId);
            e.HasOne(x => x.Task)
                .WithMany(t => t.Remarks)
                .HasForeignKey(x => x.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
