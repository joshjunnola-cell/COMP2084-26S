using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class CartItem
    {
        // PK
        // In a real application, this property would typically be used as the primary key (pk) in a database table for cart items, and it would be automatically generated when a new cart item is created. This means that when you add a new cart item to the database, the CartItemId would be assigned a unique value (usually an incrementing integer) by the database system, ensuring that each cart item can be uniquely identified.

        public int CartItemId
        {
            get; set;
        }

        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be at least 1 and realistic.")]
        public int Quantity
        {
            get; set;
        }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "The price must be greater than zero and between 0.01 and 10000.")]

        public decimal Price
        {
            get; set;
        }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        // FK: Every cart item must refer to a Book (mandatory)
        // The BookId property is an integer that serves as a foreign key (FK) to the Book class. It is marked as [Required], which means that every cart item must be associated with a book. This property will typically hold the unique identifier of the book that this cart item belongs to, ensuring that each cart item is associated with a valid book in the database.
        [Required]
        public int BookId
        {
            get; set;
        }

        // Navigation property: each cart item points to one book (mandatory from the cart side)
        // The Book property is a navigation property that allows access to the related Book object. It is marked as non-nullable (using the null-forgiving operator !) to indicate that it must always have a value. This property enables navigation from a cart item to its associated book, facilitating data access and manipulation in the application.
        [ValidateNever]
        public Book Book { get; set; } = null!;

    }
}