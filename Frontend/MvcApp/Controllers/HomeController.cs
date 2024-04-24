using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcApp.Models;
using Microsoft.AspNetCore.Http;
using System.Web;

namespace MvcApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public ActionResult NavigateToLoginScreen()
    {
        var values = new ContentResult() { Content = null, StatusCode = StatusCodes.Status100Continue };

        return View("~/Views/Home/LoginScreen.cshtml", new { Values = values });
    }


}
