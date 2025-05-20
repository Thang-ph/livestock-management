using BusinessObjects.Dtos;
using BusinessObjects.Models;

namespace DataAccess.Repository.Interfaces
{
    public interface IDiseaseRepository
    {
        Task<ListDiseases> GetListDiseases(string keyword);
        Task<DiseaseDTO> UpdateDisease(string id, DiseaseUpdateDTO model);
        Task<bool> DeleteDisease(string id);
    }
}
