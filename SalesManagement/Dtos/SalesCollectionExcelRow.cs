namespace SalesManagement.Dtos
{
    public class SalesCollectionExcelRow
    {
        public string? Date { get; set; }
        public string? Invoice { get; set; }
        public string? Party { get; set; }
        public string? Gross { get; set; }
        public string? Discount { get; set; }
        public string? NetSale { get; set; }
        public string? Vat { get; set; }
        public string? Total { get; set; }
        public string? TRNUser { get; set; }
        public string? TRNTime { get; set; }
        public string? STax { get; set; }
        public string? Pax { get; set; }
        public string? BillToPan { get; set; }
        public string? BillToMob { get; set; }
        public string? Cash { get; set; }
        public string? CreditCard { get; set; }
        public string? Credit { get; set; }
        public string? Online { get; set; }
        public string? GVoucher { get; set; }
        public string? SalesReturnVoucher { get; set; }
        public string? Complimentary { get; set; }
        public string? TransactionId { get; set; }
        public string? OrderMode { get; set; }
    }
}
