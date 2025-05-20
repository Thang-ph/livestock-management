using BusinessObjects.Dtos;

namespace DataAccess.Repository.Interfaces
{
    public interface IBarnRepository
    {
        Task<BarnInfo> GetBarnById(string id);
        Task<ListBarns> GetListBarns();
        Task<BarnInfo> CreateBarn(CreateBarnDTO createModel);
        Task<BarnInfo> UpdateBarn(string id, UpdateBarnDTO updateModel);
        Task<bool> DeleteBarn(string id);
    }
}
