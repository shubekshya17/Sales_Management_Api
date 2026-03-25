using SalesManagement.Dtos;

namespace SalesManagement.Services.Interface
{
    public interface ISalesCollectionReport
    {
        Task<SalesCollectionReportResponse> GetSalesCollectionReport(SalesCollectionReportRequest request);
    }
}
