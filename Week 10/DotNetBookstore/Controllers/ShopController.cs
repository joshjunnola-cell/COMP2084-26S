using DotNetBookstore.Data;
using DotNetBookstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBookstore.Controllers
{
    /// <summary>
    /// Public shop surface where users can browse categories and begin shopping.
    /// </summary>
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Shop or /Shop/Index
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Shop/ShopByCategory/5
        public async Task<IActionResult> ShopByCategory(int? categoryId)
        {
            if (categoryId == null)
            {
                return BadRequest();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var books = await _context.Books
                .Where(b => b.CategoryId == categoryId)
                .Include(b => b.Category)
                .OrderBy(b => b.Author)
                .ThenBy(b => b.Title)
                .ToListAsync();

            ViewData["Title"] = $"Shop - {category.Name}";
            ViewData["CategoryName"] = category.Name;

            return View("ShopByCategory", books);
        }
    }
}