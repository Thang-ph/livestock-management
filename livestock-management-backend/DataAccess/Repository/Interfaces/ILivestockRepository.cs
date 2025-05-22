using BusinessObjects.Dtos;
using BusinessObjects.Models;
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
    }
}
