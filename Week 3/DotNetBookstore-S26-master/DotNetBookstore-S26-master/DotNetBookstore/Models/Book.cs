using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class Book
    {
        // PK
        // In a real application, this property would typically be used as the primary key (pk) in a database table for books, and it would be automatically generated when a new book is created. This means that when you add a new book to the database, the BookId would be assigned a unique value (usually an incrementing integer) by the database system, ensuring that each book can be uniquely identified.
        public int BookId
        {
            get; set;
        }

        [Required]
        [MaxLength(100, ErrorMessage = "The author's name cannot exceed 100 characters.")]
        public string Author { get; set; } = string.Empty;

        [Required]
        [MaxLength(200, ErrorMessage = "The title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        // The Image property is a string that holds the URL or file path to the book's image. It is nullable, which means it can have a value of null if no image is provided for a book. This allows for flexibility in cases where an image may not be available or necessary for certain books.
        public string? Image
        {
            get; set;
        }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "The price must be greater than zero and between 0.01 and 10000.")]
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Price
        {
            get; set;
        }

        [Display(Name = "Mature Content")]
        public bool MatureContent
        {
            get; set;
        }

        // FK: Every book must have a category (mandatory)
        // The CategoryId property is an integer that serves as a foreign key (FK) to the Category class. It is marked as [Required], which means that every book must have a category assigned to it. This property will typically hold the unique identifier of the category that the book belongs to, ensuring that each book is associated with a valid category in the database.
        [Required]
        [Display(Name = "Category")]
        public int CategoryId
        {
            get; set;
        }

        // Navigation property: each book belongs to one category (mandatory from the book side)
        // The Category property is a navigation property that allows you to access the related Category object for a given book. It is marked with [ValidateNever] to indicate that it should not be validated during model validation, which can help to avoid circular references or unnecessary validation when working with related entities in a database context. This property will typically be populated by the Entity Framework when you query for books and include their related categories.
        [ValidateNever]
        public Category Category { get; set; } = null!;

        // Navigation property: Book may appear in many CartItems (optional from the book side)
        [ValidateNever]
        public ICollection<CartItem> CartItems { get; set; } = [];

        // Navigation property: Book may appear in many OrderDetails (optional from the book side)
        [ValidateNever]
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
    }
}