namespace StoryBoardBud.Data;

/// <summary>
/// Links a user to their favorited photos
/// </summary>
public class FavoritePhoto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public Guid PhotoId { get; set; }
    public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = default!;
    public Photo Photo { get; set; } = default!;
}
