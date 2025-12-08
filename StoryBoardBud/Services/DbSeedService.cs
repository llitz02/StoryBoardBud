using Microsoft.AspNetCore.Identity;
using StoryBoardBud.Data;

namespace StoryBoardBud.Services;

/// <summary>
/// Seeds the database with initial roles and users
/// </summary>
public class DbSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DbSeedService> _logger;

    /// <summary>
    /// Initializes the seed service with required dependencies
    /// </summary>
    /// <param name="context">Database context for data access</param>
    /// <param name="userManager">User manager for creating users</param>
    /// <param name="roleManager">Role manager for creating roles</param>
    /// <param name="logger">Logger for recording events</param>
    public DbSeedService(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, ILogger<DbSeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Seeds roles and default admin/test users
    /// </summary>
    /// <returns>Completed task</returns>
    public async Task SeedAsync()
    {
        try
        {
            // Ensure roles exist
            var adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
            if (!adminRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                _logger.LogInformation("Admin role created");
            }

            var userRoleExists = await _roleManager.RoleExistsAsync("User");
            if (!userRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
                _logger.LogInformation("User role created");
            }

            // Create admin user
            var adminUser = await _userManager.FindByEmailAsync("admin@storyboardbud.local");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@storyboardbud.local",
                    FullName = "Administrator",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, "AdminPassword123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Admin user created with role");
                }
                else
                {
                    _logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Create sample test user
            var testUser = await _userManager.FindByEmailAsync("testuser@storyboardbud.local");
            if (testUser == null)
            {
                testUser = new ApplicationUser
                {
                    UserName = "testuser",
                    Email = "testuser@storyboardbud.local",
                    FullName = "Test User",
                    BioDescription = "A test user for exploring StoryBoardBud",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(testUser, "TestPassword123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(testUser, "User");
                    _logger.LogInformation("Test user created");
                }
            }

            // Migrate existing users who have email as username
            await MigrateEmailUsernamesToProperUsernames();

            _logger.LogInformation("Database seeding completed");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Database seeding failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Migrates existing users who have email addresses as usernames to proper usernames
    /// </summary>
    private async Task MigrateEmailUsernamesToProperUsernames()
    {
        var users = _userManager.Users.ToList();
        
        foreach (var user in users)
        {
            // Check if username looks like an email (contains @)
            if (user.UserName != null && user.UserName.Contains("@") && user.Email != null)
            {
                // Extract username from email (part before @)
                var proposedUsername = user.Email.Split('@')[0];
                
                // Ensure username is unique
                var existingUser = await _userManager.FindByNameAsync(proposedUsername);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    // If username exists, append random suffix
                    proposedUsername = $"{proposedUsername}{new Random().Next(100, 999)}";
                }
                
                // Update username
                user.UserName = proposedUsername;
                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Migrated user {user.Email} to username {proposedUsername}");
                }
                else
                {
                    _logger.LogWarning($"Failed to migrate user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
