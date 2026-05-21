// This is the HomeController for the DotNetBookstore application. It contains actions for the Index, Privacy, and Error pages.    

using DotNetBookstore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DotNetBookstore.Controllers
{
    public class HomeController : Controller
    {
        // The Index action returns the default view for the home page of the application.
        public IActionResult Index()
        {
            // You can add any logic here that you want to execute when the home page is accessed.
            // For example, you could retrieve some data from a database or perform some calculations.
            return View();
        }

        // The Privacy action returns the view for the privacy policy page.
        public IActionResult Privacy()
        {
            return View();
        }

        // The Error action returns the view for the error page, including the error details.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
