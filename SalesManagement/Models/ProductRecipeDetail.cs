namespace SalesManagement.Models
{
    public class ProductRecipeDetail
    {
        public int Id { get; set; }
        public int ProductRecipeId { get; set; }
        public int ProductIngredientId { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
        public ProductRecipe ProductRecipe { get; set; }
    }
}
