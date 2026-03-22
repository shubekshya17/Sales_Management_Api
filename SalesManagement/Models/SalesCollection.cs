using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagement.Models
{
    public class SalesCollection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string Invoice { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Party { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gross { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSale { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Vat { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [MaxLength(100)]
        public string? TRNUser { get; set; }

        [MaxLength(50)]
        public string? TRNTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal STax { get; set; }

        public int? Pax { get; set; }

        [MaxLength(20)]
        public string? BillToPan { get; set; }

        [MaxLength(20)]
        public string? BillToMob { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cash { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CreditCard { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Credit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Online { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GVoucher { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SalesReturnVoucher { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Complimentary { get; set; }

        [MaxLength(100)]
        public string? TransactionId { get; set; }

        [MaxLength(50)]
        public string? OrderMode { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
