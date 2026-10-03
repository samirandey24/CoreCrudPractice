using System.ComponentModel.DataAnnotations;

namespace CoreCrudWithJwt.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string ProductName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
