using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using System.Data;


namespace DataAccess.Repository.Interfaces
{
    public interface IProcurementRepository
    {

        Task<ListProcurements> GetListProcurements(ListProcurementsFilter filter);

        Task<ProcurementSummary> CreateProcurementPackage(CreateProcurementPackageRequest request);

        Task<ProcurementGeneralInfo> GetProcurementGeneralInfo(string id);

        Task<bool> CompleteProcurementPackage(ProcurementStatus status);
        Task<bool> CancelProcurementPackage(ProcurementStatus status);

        Task<ProcurementSummary> UpdateProcurementPackage(UpdateProcurementPackageRequest request);

        Task<string> GetTemplateListCustomers(string id);

        Task ImportListCustomers(string procurementId, string requestedBy, IFormFile file);

        public IQueryable<Livestock> GetSuggestLivestock(string procurementID);

        Task<CreateExportDetail> CreateExportDetail(CreateExportDetail request);

        Task<UpdateExportDetail> UpdateExportDetail(UpdateExportDetail request);

        Task<ListExportDetails> GetListExportDetails(string id);

        Task<DataTable> GetEmpData(string procurementID);
        Task<List<HandOverProcessProcurement>> GetProcessHandOverProcurementList();
        Task<bool> AcceptProcurementPackage(ProcurementStatus status);
        Task<ProcurementOverview> GetPrucrementPreview();
    }
}
