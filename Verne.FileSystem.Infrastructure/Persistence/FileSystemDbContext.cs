using Microsoft.EntityFrameworkCore;
using Verne.FileSystem.Core.Entities;

namespace Verne.FileSystem.Infrastructure.Persistence;

public class FileSystemDbContext : DbContext
{
    private DbContextOptions<FileSystemDbContext> _options;
    public FileSystemDbContext(DbContextOptions<FileSystemDbContext> options) : base()
    {
        _options = options;
    }
    public DbSet<FileSystemNode> Nodes { get; set; }

     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<FileSystemNode>()
            .HasMany(n => n.Children) 
            .WithOne(n => n.Parent) 
            .HasForeignKey(n => n.ParentId) 
            .OnDelete(DeleteBehavior.Cascade);
    }
}