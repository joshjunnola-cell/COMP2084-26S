// This code defines a simple Category class with two properties: CategoryId and Name.  while the Name is a string that holds the name of the category. This class is part of the DotNetBookstore.Models namespace, which suggests it is used in a bookstore application to represent different categories of books.

namespace DotNetBookstore.Models
{
    public class Category
    {
        // In a real application, this property would typically be used as the primary key (pk) in a database table for categories, and it would be automatically generated when a new category is created. This means that when you add a new category to the database, the CategoryId would be assigned a unique value (usually an incrementing integer) by the database system, ensuring that each category can be uniquely identified.

        // Naming convention: the property is named CategoryId, which follows the common convention for primary key properties in C#. This convention helps to clearly indicate that this property is intended to be used as a unique identifier for instances of the Category class (it can also be named Id, but using CategoryId can help to avoid confusion when there are multiple classes with primary key properties).
        public int CategoryId
        {
            get; set;
        }
        // The Name property is a string that holds the name of the category. It has both a getter and a setter, allowing you to read and modify its value. It is initialized to an empty string to ensure it has a default value.
        public string Name { get; set; } = string.Empty;
    }
}
