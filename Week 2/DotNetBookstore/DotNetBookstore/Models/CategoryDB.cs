namespace DotNetBookstore.Models
{
    public static class CategoryDB
    {
        private static List<Category> _categories = new List<Category>
        {
            new Category { CategoryId = 1, Name = "Fiction" },
            new Category { CategoryId = 2, Name = "Science" },
            new Category { CategoryId = 3, Name = "History" },
            new Category { CategoryId = 4, Name = "Science-Fiction"}
        };

        public static List<Category> GetCategories() => _categories;

        public static Category? GetCategory(int id) =>
            _categories.FirstOrDefault(c => c.CategoryId == id);
    }
}
