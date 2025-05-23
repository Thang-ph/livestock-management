using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos;
using BusinessObjects.Models;

namespace DataAccess.Repository.Interfaces
{
    public interface IInsuranceRequestRepository
    {
        Task<ListInsurenceRequestOverviewDTO> GetPendingOverviewList();
        Task<ListInsurenceRequestDTO> GetInsurenceList(InsurenceFilter filter);
        Task<InsurenceRequestInfoDTO> GetInsurenceRequestInfo(string id);
        Task<InsurenceRequestDTO> CreateInsurenceRequest(CreateInsurenceDTO createDto);
        ListInsuranceStatusDTO GetAllStatusInsurence();
        Task<InsurenceRequestDTO> ChangeStatusInsurance(ChangeStatusInsuranceDto insuranceDto);
        Task<CreateInsurenceDTO> CreateInsurenceRequestWithScan(CreateInsurenceQrDTO createDto);
        Task<InsurenceRequestDTO> UpdateNewLivestockInsurenceRequest(UpdateInsuranceLivestockDto updateDto);
        Task<InsurenceRequestDTO> RemoveNewLivestockInsuranceRequest(RemoveInsuranceLivestockDto updateDto);
        Task<InsurenceRequestDTO> UpdateInfoInsuranceRequest(UpdateInsuranceRequestInfoDto updateDto);
        Task<InsurenceRequestDTO> ApproveInsuranceRequest(RemoveInsuranceLivestockDto data);
        Task<InsurenceRequestDTO> RejectInsuranceRequest(RejectInsuranceDto data);
        Task<InsurenceRequestDTO> TransferInsuranceRequest(RemoveInsuranceLivestockDto data);
        Task<VaccinationProcurmentDto> GetDataProcurmentByInsurance(string id);
    }
}
