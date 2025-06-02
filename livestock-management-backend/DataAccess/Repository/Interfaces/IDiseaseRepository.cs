using BusinessObjects.Dtos;
using BusinessObjects.Models;

namespace DataAccess.Repository.Interfaces
{
    public interface IDiseaseRepository
    {
        Task<ListDiseases> GetListDiseases(string keyword);
        Task<DiseaseDTO> UpdateDisease(string id, DiseaseUpdateDTO model);
        Task<bool> DeleteDisease(string id);
        Task<DiseaseDTO?> GetByIdAsync(string id);
        Task<DiseaseDTO> CreateDisease(DiseaseUpdateDTO model);
        Task<StatsDiseaseSummary> GetStatsDiseaseByMonth(GetStatsDiseaseByMonthFilter filter);
        Task<VaccinationRatioSummary> GetVaccinatedRatios();
        Task<DiseaseRatioSummary> GetDiseaseRatios();
        Task<int> GetCurrentDiseaseQuantity();
    }
}
