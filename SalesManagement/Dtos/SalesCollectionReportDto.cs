namespace SalesManagement.Dtos
{
    public class SalesCollectionReportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class AmountCalculation
    {
        public decimal TotalCash { get; set; }
        public decimal TotalCreditCard { get; set; }
        public decimal TotalOnline { get; set; }
        public decimal TotalCredit { get; set; }
    }
    public class SalesCollectionReportResponse
    {
        public AmountCalculation AmountCalculation { get; set; } = new();
        public decimal NetTotal { get; set; }   
        public decimal Discount { get; set; }  
        public decimal Vat { get; set; }        
        public decimal Total { get; set; }     
    }

    public class PaymentDetailRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
    public class SalesCollectionDetail
    {
        public DateTime Date { get; set; }
        public string Invoice { get; set; } = string.Empty;
        public string Party { get; set; } = string.Empty;
        public decimal Gross { get; set; }
        public decimal Discount { get; set; }
        public decimal NetSale { get; set; }
        public decimal Vat { get; set; }
        public decimal Total { get; set; } 
        public string TRNUser { get; set; } = string.Empty;
        public string TRNTime { get; set; } = string.Empty;
        public decimal STax { get; set; }
        public int Pax { get; set; }
        public string BillToPan { get; set; } = string.Empty;
        public string BillToMob { get; set; } = string.Empty;
        public decimal Cash { get; set; }
        public decimal CreditCard { get; set; }
        public decimal Credit { get; set; }
        public decimal Online { get; set; }
        public decimal GVoucher { get; set; }
        public decimal SalesReturnVoucher { get; set; }
        public decimal Complimentary { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string OrderMode { get; set; } = string.Empty;

    }
}
