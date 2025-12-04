using Microsoft.AspNetCore.Identity;
using StoryBoardBud.Data;

namespace StoryBoardBud.Services;

public class DbSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DbSeedService> _logger;

    public DbSeedService(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, ILogger<DbSeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

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

            _logger.LogInformation("Database seeding completed");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Database seeding failed: {ex.Message}");
        }
    }
}
