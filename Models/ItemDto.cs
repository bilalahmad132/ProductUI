using DocumentFormat.OpenXml.Drawing.Charts;

namespace WebProductProject.Models
{
    public class ItemDto
    {
        public int ItemId { get; set; } // Add ItemId
        public string Name { get; set; } // Product name
        public string Desc { get; set; } // Product description
        public decimal Price { get; set; } // Product price

        public ICollection<Order> Orders { get; set; }

    }
}
