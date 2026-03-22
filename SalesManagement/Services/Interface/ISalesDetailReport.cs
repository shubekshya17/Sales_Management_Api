using SalesManagement.Dtos;

namespace SalesManagement.Services.Interface
{
    public interface ISalesDetailReport
    {
        Task<SalesDetailReportResponse> GetSalesDetailReport(SalesDetailReportRequest request);
    }
}
