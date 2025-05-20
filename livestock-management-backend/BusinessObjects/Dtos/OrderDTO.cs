using BusinessObjects.ConfigModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace BusinessObjects.Dtos
{
    public class ListOrders : ResponseListModel<OrderSummary>
    {

    }

    public class OrderSummary
    {
        public string Id { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public int Total { get; set; }   
        public int Received { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public order_status Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ListOrderFilter : CommonListFilterModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public order_status? Status { get; set; }
    }

}
