using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface IOrderRepository
    {
        Task<ListOrders> GetListOrder(ListOrderFilter filter);
        Task<OrderDetailsDTO> CreateOrder(CreateOrderDTO createOrder);
        Task<OrderDetailsDTO> UpdateOrder(string orserId, UpdateOrderDTO updateOrder);
        Task<OrderDetailsDTO> GetOrderDetailsById(string orderId);
        Task<bool> CancelOrder(string orderId, string? requestedBy);
        Task<bool> CompletelOrder(string orderId, string? requestedBy);
        Task<LivestockBatchImportInfo> AddLivestockToOrder(string orderId, AddLivestockToOrderDTO model);
        Task<bool> DeleteLivestockFromOrder(string orderDetailsId);
        Task<ListOrderLivestocks> GetListLivestockInOrder(string orderId, ListLivestockFilter filter);
        Task<bool> RequestChoose(string orderId);
        Task<bool> RemoveRequestChoose(string orderId);
        Task<bool> RequestExport(string orderId);
        Task ImportListChosedLivestock(string orderId, string requestedBy, IFormFile file);
        Task <string> GetTemplateToChooseLivestock(string orderId);
        Task <string> GetReportedFile(string orderId);
        Task <bool> ConfirmExport (string orderId, string? requestedBy);
        Task<ListOrders> GetListOrderToChoose();
        Task<ListOrderExport> GetListOrderToExport();
    }
}
