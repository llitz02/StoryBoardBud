using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StoryBoardBud.Data;
using StoryBoardBud.Services;

/// <summary>
/// This is main entry point of the StoryBoardBud application, story board bud 
/// is a web application for managing story boards and photo collections.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// this is the service configuration section, in charge manging dependencies
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<DbSeedService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// the following section is responsible for initializing the 
//database and seeding initial data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seedService = scope.ServiceProvider.GetRequiredService<DbSeedService>();
    
    await dbContext.Database.MigrateAsync();
    await seedService.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
