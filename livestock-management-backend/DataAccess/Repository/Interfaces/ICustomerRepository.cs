using BusinessObjects.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface ICustomerRepository
    {
        Task<CustomerInfo> GetCustomerInfo(string customerId);
        Task<CustomerInfo> AddCustomer(AddCustomerDTO model);

    }
}
