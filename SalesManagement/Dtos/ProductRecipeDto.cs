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
}
