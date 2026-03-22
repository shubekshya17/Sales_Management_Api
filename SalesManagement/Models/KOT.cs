using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagement.Models
{
    public class KOT
    {
        [Key]
        public int Id { get; set; }
        public string? KOTNO { get; set; }
        public string? KOTTIME { get; set; }
        public string? TABLENO { get; set; }
        public string? WAITER { get; set; }
        public string? WAITERNAME { get; set; }
        public string? ITEMCODE { get; set; }
        public string? ITEMDESCRIPTION { get; set; }
        public decimal QUANTITY { get; set; }
        public string? UNIT { get; set; }
        public string? REMARKS { get; set; }
        public string? BILLED { get; set; }
        public string? BILLNO { get; set; }
        public string? TRANSFERKOT { get; set; }
        public string? MERGEKOT { get; set; }
        public string? SPLITKOT { get; set; }
        public string? CANCELBY { get; set; }
        public string? CANCELREMARKS { get; set; }
        public string? FLG { get; set; }
        public string? ISBARITEM { get; set; }
        public string? KOTID { get; set; }
        public string? RowHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
