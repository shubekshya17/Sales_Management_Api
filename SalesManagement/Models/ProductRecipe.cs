using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagement.Models
{
    public class ProductRecipe
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public CategoryRange CategoryRange { get; set; }
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ProductRecipeDetail> Details { get; set; }
    }
}
