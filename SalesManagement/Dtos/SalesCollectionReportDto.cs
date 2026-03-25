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
}
