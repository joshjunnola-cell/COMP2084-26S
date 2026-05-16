using Microsoft.AspNetCore.Mvc;
using DotNetBookstore.Models;

namespace DotNetBookstore.Controllers
{
    public class CategoriesController : Controller
    {

        public IActionResult Greet(string name, int times = 1)
        {
            ViewBag.Name = name;
            ViewBag.Times = times;
            return View();
        }
        
        public IActionResult List()
        {
            ViewData["Message"] = "Welcome to the Categories PAge!";
            ViewBag.Note = "Data recieved from ViewBag";

            var categories = CategoryDB.GetCategories();
            return View(categories);
        }

        public IActionResult Featured()
        {
            var featured = new Category
            {
                CategoryId = 99,
                Name = "Featured Category"
            };
            return View(featured);
        }

        public IActionResult Detail(int id)
        {
            var category = CategoryDB.GetCategory(id);
            return View(category);
        }
    }
}
