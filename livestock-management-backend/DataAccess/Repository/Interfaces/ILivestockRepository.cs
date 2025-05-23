using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Interfaces
{
    public interface ILivestockRepository
    {
        Task<ListLivestocks> GetListLivestocks(ListLivestocksFilter filter);

        Task<LivestockGeneralInfo> GetLivestockGeneralInfo(string id);

        Task<LivestockVaccinationHistory> GetLivestockVaccinationHistory(string id);

        Task<LivestockSicknessHistory> GetLivestockSicknessHistory(string id);

        Task<LivestockSummary> GetLivestockSummaryInfo(string id);

        Task<byte[]> ExportListNoCodeLivestockExcel();

        Task<LivestockGeneralInfo> GetLivestockGeneralInfo(string inspectionCode, specie_type specieType);

        Task<LivestockSummary> GetLiveStockIdByInspectionCodeAndType(LivestockIdFindDTO model);

        Task<DashboardLivestock> GetDashboarLivestock();

        Task<string> GetDiseaseReport();

        Task<string> GetWeightBySpecieReport();

        Task<ListLivestockSummary> ListLivestockSummary();

        Task<string> GetListLivestocksReport();

        Task<string> GetRecordLivestockStatusTemplate();

        Task ImportRecordLivestockStatusFile(string requestedBy, IFormFile file);

        Task<int> GetTotalEmptyRecords();

        Task<string> GetEmptyQrCodesFile();

        Task<bool> CreateEmptyLivestockRecords(string requestedBy, int quantity);

        Task<string> GetRecordLivestockStatInformationTemplate();

        Task ImportRecordLivestockInformationFile(string requestedBy, IFormFile file);

        Task ChangeLivestockStatus(string requestedBy, string[] livestockIds, livestock_status status);

        Task<LivestockDetails> GetLivestockDetails(GetLivestockDetailsRequest request);

        Task UpdateLivestockDetails(UpdateLivestockDetailsRequest request);

        Task RecordLivestockDiseases(RecordLivstockDiseases request);
    }
}
