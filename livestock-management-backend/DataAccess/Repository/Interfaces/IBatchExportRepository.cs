using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface IBatchExportRepository
    {
        Task<bool> DeleteCustomerFromBatchExport(string batchExportID);
        Task<ListCustomers> GetListCustomers(string id, CustomersFliter filter);

        Task ImportCustomer(string procurementId, string requestedBy, IFormFile file);
        Task<BatchExport> UpdateCustomerFromBatchExport(string batchExportId, UpdateCustomerBatchExportDTO updateCustomerBatchExportDTO);
        Task<BatchExport> AddCustomerFromBatchExport(AddCustomerBatchExportDTO addCustomerBatchExportDTO);
        Task<bool> CanAddCustomerInBatchExport(string procurementId);
        Task<bool> DeleteLivestockFromBatchExportDetail(string batchExportDetailId);
        Task<bool> AddLivestockToBatchExportDetail( BatchExportDetailAddDTO batchExportDetailAddDTO);
        Task<BatchExportDetail> ChangeLivestockToBatchExportDetail(string batchExportDetailId, BatchExportDetailChangeDTO batchExportDetailChangeDTO);
        Task<BatchExportDetail> UpdateBatchExportDetail(BatchExportDetailUpdateDTO batchExportDetailChangeDTO);
        Task<bool> CanChangeLivestockInBatchExportDetail(string batchExportDetailId);
        Task<bool> ConfirmHandoverBatchExportDetail(string livestockId, string UpdatedBy);
        Task<bool> AddLivestockToBatchExportDetailByInspectionCode(BatchExportDetailAddDTOByInspectionCode batchExportDetailAdd);
        Task<bool> ConfirmHandoverBatchExportDetailByInspectionCode(string inspectionCode, LmsConstants.specie_type specieType, string updatedBy);
    }
}
