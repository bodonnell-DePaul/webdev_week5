using TodoListApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace TodoListApi.Data;


public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Todo entity
        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasMaxLength(36);

            entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.Description)
                .HasMaxLength(1000);

            entity.Property(t => t.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.Priority)
                .HasConversion<string>();

            // Configure Tags as JSON (for SQLite/SQL Server)
            entity.Property(t => t.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                );

            // Add this to the Todo entity configuration
            entity.Property(t => t.AssignedTo)
                .HasMaxLength(100);
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .HasMaxLength(36);

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Description)
                .HasMaxLength(500);

            entity.Property(c => c.Color)
                .IsRequired()
                .HasMaxLength(7);
        });

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = "1", Name = "Academic", Description = "School and university related tasks", Color = "#3b82f6" },
            new Category { Id = "2", Name = "Personal", Description = "Personal life and household tasks", Color = "#10b981" },
            new Category { Id = "3", Name = "Work", Description = "Professional and career related tasks", Color = "#f59e0b" },
            new Category { Id = "4", Name = "Health", Description = "Health and fitness related activities", Color = "#ef4444" },
            new Category { Id = "5", Name = "Learning", Description = "Learning and skill development", Color = "#8b5cf6" }
        );

        // Seed Todos
        modelBuilder.Entity<Todo>().HasData(
            new Todo
            {
                Id = "1",
                Title = "Complete project proposal",
                Description = "Write and submit the final project proposal for CSC436",
                Priority = Priority.High,
                Category = "Academic",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(11),
                Tags = ["project", "academic", "deadline"]//JsonSerializer.Serialize(new List<string> { "project", "academic", "deadline" })
            },
            new Todo
            {
                Id = "2",
                Title = "Grocery shopping",
                Description = "Buy groceries for the week including fruits and vegetables",
                Priority = Priority.Medium,
                Category = "Personal",
                IsCompleted = true,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(1),
                Tags = ["shopping", "food", "weekly"]//JsonSerializer.Serialize(new List<string> { "shopping", "food", "weekly" })
            },
            new Todo
            {
                Id = "3",
                Title = "Team meeting preparation",
                Description = "Prepare slides and agenda for the weekly team meeting",
                Priority = Priority.High,
                Category = "Work",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(4),
                Tags = ["meeting", "presentation", "team"]//JsonSerializer.Serialize(new List<string> { "meeting", "presentation", "team" })
            },
            new Todo
            {
                Id = "4",
                Title = "Exercise routine",
                Description = "Complete 30-minute workout including cardio and strength training",
                Priority = Priority.Medium,
                Category = "Health",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow,
                Tags = new List<string> { "fitness", "health", "routine" }
            },
            new Todo
            {
                Id = "5",
                Title = "Read React documentation",
                Description = "Study advanced React patterns including Context API and custom hooks",
                Priority = Priority.Low,
                Category = "Learning",
                IsCompleted = false,
                CreatedDate = DateTime.UtcNow,
                Tags = new List<string> { "learning", "react", "documentation" }
            }
        );
    }
}

/// if you db is not updating and the data is being held in wal
    // using (var context = new YourDbContext())
    // {
    //     context.Database.ExecuteSqlRaw("PRAGMA wal_checkpoint(TRUNCATE);");
    // }