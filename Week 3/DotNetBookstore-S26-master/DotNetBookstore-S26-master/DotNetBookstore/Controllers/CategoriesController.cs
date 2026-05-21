using Microsoft.AspNetCore.Mvc;
using DotNetBookstore.Models;

namespace DotNetBookstore.Controllers
{
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            // You can add any logic here that you want to execute when the categories page is accessed.
            // This is where you would typically retrieve data from a database or perform any other operations that are necessary for the page to function properly. You can also pass data to the view using the ViewBag or ViewData properties, or by creating a view model and passing it to the view.
            // This could include things like retrieving data from a database, performing calculations, or any other operations that are necessary for the page to function properly.

            // For demonstration purposes, we'll just create a list of categories in memory and pass it to the view.         
            //var categories = new List<Models.Category>
            // {
            //     new Models.Category { CategoryId = 1, Name = "Novels" },
            //     new Models.Category { CategoryId = 2, Name = "History" },
            //     new Models.Category { CategoryId = 3, Name = "Science Fiction" }
            // };

            // In a real application, you would typically retrieve the categories from a database
            // For demonstration purposes, we'll just create a list of categories in memory
            var categories = new List<Category>();
            for (int i = 1; i <= 10; i++)
            {
                categories.Add(new Category { CategoryId = i, Name = $"Category {i}" });
            }
            // Pass the list of categories to the view (strongly-typed)
            // model binding: the view will be strongly-typed to a list of Category objects, and we will pass the list of categories to the view using the View method. This allows us to use the categories in the view to display them to the user.
            return View(categories);
        }

        // This action method will be called when the user clicks on a category link. It will receive the category name as a parameter and can use it to retrieve the relevant books from the database or perform any other necessary operations.
        public IActionResult Browse(string category) 
        {
            // A simple error handling: if the category parameter is null or empty, we can return a BadRequest result or redirect to an error page.
            if (string.IsNullOrEmpty(category))
            {
                // Return a BadRequest result with a message indicating that the category cannot be null or empty.
                // This will result in a 400 Bad Request response being sent to the client, along with the specified error message.
                // You can also choose to redirect to an error page instead of returning a BadRequest result, depending on how you want to handle errors in your application.
                // for demonstration purposes, we'll just return a BadRequest result with a message indicating that the category cannot be null or empty (in production, you might want to log this error or handle it differently).
                //return BadRequest("Category cannot be null or empty.");

                // Redirect to the Index action of the CategoriesController, which will display the list of categories. This is a common way to handle errors in a user-friendly way, by redirecting the user back to a relevant page instead of showing an error message.
                return RedirectToAction("Index");
            }

            // passing the category name to the view using ViewBag (weakly-typed)
            ViewBag.Category = category;
            // In a real application, you would typically retrieve the books for the specified category from a database and pass them to the view. For demonstration purposes, we'll just pass the category name to the view using ViewBag.
            return View();
        }
    }
}