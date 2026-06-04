using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class OrderDetail
    {
        // PK
        // In a real application, this property would typically be used as the primary key (pk) in a database table for order details, and it would be automatically generated when a new order detail is created. This means that when you add a new order detail to the database, the OrderDetailId would be assigned a unique value (usually an incrementing integer) by the database system, ensuring that each order detail can be uniquely identified.
        public int OrderDetailId
        {
            get; set;
        }

        [Required]
        public int Quantity
        {
            get; set;
        }

        [Required]
        public decimal Price
        {
            get; set;
        }

        // FK: must refer to an order (mandatory)
        // The OrderId property is an integer that serves as a foreign key (FK) to the Order class. It is marked as [Required], which means that every order detail must be associated with an order. This property will typically hold the unique identifier of the order that this detail belongs to, ensuring that each order detail is associated with a valid order in the database.
        [Required]
        public int OrderId
        {
            get; set;
        }

        // Navigation property: each detail is for one order (mandatory from the detail side)
        // The Order property is a navigation property that allows access to the related Order object. It is marked as non-nullable (using the null-forgiving operator !) to indicate that it must always have a value. This property enables navigation from an order detail to its associated order, facilitating data access and manipulation in the application.
        public Order Order { get; set; } = null!;

        // FK: must refer to a book (mandatory)
        // The BookId property is an integer that serves as a foreign key (FK) to the Book class. It is marked as [Required], which means that every order detail must be associated with a book. This property will typically hold the unique identifier of the book that this detail belongs to, ensuring that each order detail is associated with a valid book in the database.
        [Required]
        public int BookId
        {
            get; set;
        }

        // Navigation property: each detail is for one book (mandatory from the detail side)
        // The Book property is a navigation property that allows access to the related Book object. It is marked as non-nullable (using the null-forgiving operator !) to indicate that it must always have a value. This property enables navigation from an order detail to its associated book, facilitating data access and manipulation in the application.
        [ValidateNever]
        public Book Book { get; set; } = null!;
    }
}