using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Interfaces
{
    public interface IImportRepository
    {
        Task<ImportBatchDetails> GetImportBatchDetails(string id);
        Task<ListImportBatches> GetListImportBatches(ListImportBatchesFilter filter);
        Task<bool> StatusComplete(BatchImportStatus status);
        Task<bool> StatusCancel(BatchImportStatus status);
        Task<BatchImport> CreateBatchImport(BatchImportCreate batchImportCreate);
        Task<BatchImport> UpdateBatchImportAsync(string id, BatchImportUpdate batchImportUpdate);
        Task<bool> DeleteLivestockFromBatchImport(string deleteId);
        Task<LivestockBatchImportInfo> GetLivestockInfoInBatchImport(string id);
        Task<bool> SetLivestockDead(string bathcImportDetailsId, string requestedBy);
        Task<LivestockBatchImportInfo> AddLivestockToDetails(string batchImportId, AddImportLivestockDTO livestockAddModel);
        Task<LivestockBatchImportInfo> UpdateLivestockInDetails(string id, UpdateImportLivestockDTO livestockUpdateModel);
        Task<bool> AddToPinImportBatch(string batchImportId, string requestedBy);
        Task<bool> RemoveFromPinImportBatch(string pinnedImportId, string requestedBy);
        Task<ListPinnedImportBatches> GetListPinnedBatcImport(string userId);
        Task<ListOverDueImportBatches> GetListOverDueBatchImport();
        Task<ListMissingImportBatches> GetListMissingBatchImport();
        Task<ListNearDueImportBatches> GetListNearDueBatchImport(int num);
        Task<ListUpcomingImportBatches> GetListUpcomingBatchImport(int num);
        //Task<BatchImportScanDTO> GetBatchImportScanDetails(string batchImportId);
        Task<ListSearchHistory> GetListSearchHistory(string userId);
        Task<bool> ConfirmImportedToBarn(string livestockId, string requestedBy);
        Task<LivestockBatchImportScanInfo> GetLivestockScanInfo(string livestockId);
        Task<LivestockBatchImportScanInfo> ConfrimReplaceDeadLiveStock(string batchImportId, AddImportLivestockDTO livestockAddModel);
        Task<bool> SetForSaleLivestock(string livestockId, string requestedBy);
        Task<ListImportBatchesForAdmin> GetListBatchImportForAdmin(ListImportBatchesFilter filter);
    }
}
