namespace WebProductProject.Models
{
    public class OrderDto
    {
        public int ItemId { get; set; } // Item ID to identify the product
        public int Quantity { get; set; } // Quantity of the product ordered
        public decimal TotalPrice { get; set; } // Total price based on quantity
        public string Username { get; set; } // Username of the customer placing the order
    }
}
