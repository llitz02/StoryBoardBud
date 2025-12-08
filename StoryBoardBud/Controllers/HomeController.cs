using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StoryBoardBud.Models;

namespace StoryBoardBud.Controllers;

/// <summary>
/// Handles home page and general application views
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Initializes the HomeController with logging support
    /// </summary>
    /// <param name="logger">Logger for recording application events</param>
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Displays the home page
    /// </summary>
    /// <returns>The home page view</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays the privacy policy page
    /// </summary>
    /// <returns>The privacy policy view</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Displays the error page with request details
    /// </summary>
    /// <returns>The error view with error information</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
