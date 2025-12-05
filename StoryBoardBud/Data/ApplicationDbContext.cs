using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StoryBoardBud.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Board> Boards { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<BoardItem> BoardItems { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<FavoritePhoto> FavoritePhotos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Boards)
            .WithOne(b => b.Owner)
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.UploadedPhotos)
            .WithOne(p => p.UploadedBy)
            .HasForeignKey(p => p.UploadedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Reports)
            .WithOne(r => r.ReportedBy)
            .HasForeignKey(r => r.ReportedById)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Board>()
            .HasMany(b => b.Items)
            .WithOne(bi => bi.Board)
            .HasForeignKey(bi => bi.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Photo>()
            .HasMany(p => p.BoardItems)
            .WithOne(bi => bi.Photo)
            .HasForeignKey(bi => bi.PhotoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Photo>()
            .HasMany(p => p.Reports)
            .WithOne(r => r.Photo)
            .HasForeignKey(r => r.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Report>()
            .HasOne(r => r.ReviewedBy)
            .WithMany()
            .HasForeignKey(r => r.ReviewedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.FavoritePhotos)
            .WithOne(f => f.User)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Photo>()
            .HasMany(p => p.FavoritedByUsers)
            .WithOne(f => f.Photo)
            .HasForeignKey(f => f.PhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Composite unique index to prevent duplicate favorites
        builder.Entity<FavoritePhoto>()
            .HasIndex(f => new { f.UserId, f.PhotoId })
            .IsUnique();
    }
}
