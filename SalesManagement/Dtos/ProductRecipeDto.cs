namespace SalesManagement.Dtos
{
    public class CreateProductRecipeDto
    {
        public int CategoryId { get; set; }
        public int ProductId { get; set; }
        public List<ProductRecipeIngredientDto> Ingredients { get; set; }
    }
    public class ProductRecipeIngredientDto
    {
        public int ProductIngredientId { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
    }
    public class ProductRecipeListVM
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public List<RecipeIngredientVM> Ingredients { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class RecipeIngredientVM
    {
        public int ProductRecipeDetailId { get; set; }
        public int ProductIngredientId { get; set; }
        public string IngredientName { get; set; }
        public string UnitName { get; set; }
        public decimal Quantity { get; set; }
    }

}
