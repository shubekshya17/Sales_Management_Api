namespace SalesManagement.Models
{
    public class CategoryRange
    {
        public int Id { get; set; }

        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
