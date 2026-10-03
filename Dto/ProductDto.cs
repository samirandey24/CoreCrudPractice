using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CoreCrudWithJwt.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string ProductName { get; set; } = null!;
        public string Description { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
