using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Constants;
using static BusinessObjects.Constants.LmsConstants;
namespace DataAccess.Repository.Interfaces
{
    public interface IBatchVacinationRepository
    {
        Task<BatchVaccination> CreateBatchVacinationDetail(BatchVacinationCreate batchVacinationCreate);
        Task<BatchVaccination> UpdateBatchVacinationAsync(BatchVacinationUpdate batchVacinationUpdate, string batchVaccinationId);
        Task<ListVaccination> GetListVaccinations(ListVaccinationsFliter filter);
        Task<VaccinationGeneral> GetVaccinationGeneralInfo(string id);
        Task<ListLivestocksVaccination> GetListLivestockVaccination(string id, ListLivestockVaccination filter);
        Task<bool> CancelVaccinationBatch(ChangeVaccinationBatchStatus request);
        Task<bool> CompleteVaccinationBatch(ChangeVaccinationBatchStatus request);
        Task<LivestockVaccinationInfo> GetLivestockInfo(ScanLivestockQrCode request);
        Task<bool> AddToVaccinationBatch(AddLivestockToVaccinationBatch request);
        Task<LivestockVaccination> AddLivestockVacinationToVacinationBatch(LivestockVaccinationAdd livestockVaccinationAdd);
        Task<LivestockVaccineInfoById> GetLivestockVaccinationByID(string livestockId);
        Task<List<UserDTO>> GetStaffAndManagerUserAsync(DateTime dateSchedule);
        Task<bool> DeleteLiveStockVaccination(string id);
        Task<Stream> ExportTemplateVaccinationBatch();
        Task<bool> ImportListLivestock(string batchImportId, string requestedBy, IFormFile file);
        Task<List<ListRequireVaccinationProcurement>> GetListProcurementRequireVaccination(string? procurementSearch, OrderBy? orderBy,DateTime? fromDate,DateTime? toDate);
        Task<List<ListSuggestReVaccination>> GetListSuggestReVaccination(string? search,string? medicineId, string? diseaseId, DateTime? fromDate, DateTime? toDate);
        Task<List<ListFutureBatchVaccination>> GetListFutureVaccination(string? search, string? diseaeId, string? conductId, DateTime? fromDate, DateTime? toDate);
        Task<RequireVaccinationProcurementDetail> GetProcurementRequireVaccinationDetail(string procurementId);
        Task<LivestockRequireVaccinationProcurement> GetLivestockRequirementForProcurement(string livestockId, string procurementId);
        Task<LivestockRequireVaccinationProcurement> GetLivestockRequirementForProcurement(string inspectionCode, specie_type specieType,string procurementId);
        Task<LivestockVaccination> AddLivestockVaccinationToVaccinationBatch(LivestockVaccinationAddByInspectionCode livestockVaccinationAddByInspectionCode);
    }
}
