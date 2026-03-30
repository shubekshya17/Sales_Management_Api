using System.ComponentModel.DataAnnotations;

namespace SalesManagement.Models
{
    public class ProductIngredient
    {
        [Key]
        public int Id { get; set; }
        public string Ingredient { get; set; } 
        public string Unit { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
