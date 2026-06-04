using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DotNetBookstore.Models
{
    public class Order
    {
        // PK
        // In a real application, this property would typically be used as the primary key (pk) in a database table for orders, and it would be automatically generated when a new order is created. This means that when you add a new order to the database, the OrderId would be assigned a unique value (usually an incrementing integer) by the database system, ensuring that each order can be uniquely identified.
        public int OrderId
        {
            get; set;
        }

        [Display(Name = "Order Date")]
        public DateTime OrderDate
        {
            get; set;
        }

        [Display(Name = "Order Total")]
        public decimal OrderTotal
        {
            get; set;
        }

        [Required]
        [Display(Name = "First Name")]
        [MaxLength(50, ErrorMessage = "The first name cannot exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [MaxLength(50, ErrorMessage = "The last name cannot exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150, ErrorMessage = "The address cannot exceed 150 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(50, ErrorMessage = "The city cannot exceed 50 characters.")]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(2, ErrorMessage = "The province cannot exceed 2 characters.")]
        public string Province { get; set; } = string.Empty;

        [Required]
        [MaxLength(10, ErrorMessage = "The postal code cannot exceed 10 characters.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(15, ErrorMessage = "The phone number cannot exceed 15 characters.")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Email")]
        [MaxLength(100, ErrorMessage = "The email cannot exceed 100 characters.")]
        public string CustomerId { get; set; } = string.Empty;

        // Navigation property: Order can have many details (optional from the order side)
        // This property is a collection of OrderDetail objects that belong to this order. It is initialized to an empty collection (using the array initializer syntax) to ensure it is not null when accessed. The [ValidateNever] attribute indicates that this property should not be validated during model validation, which can be useful to avoid circular references or unnecessary validation when working with related entities in a database context.
        [ValidateNever]
        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
    }
}