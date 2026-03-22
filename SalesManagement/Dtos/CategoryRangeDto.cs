namespace SalesManagement.Dtos
{
    public class CreateCategoryRangeDto
    {
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
    public class CategoryRangeDto
    {
        public int Id { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
