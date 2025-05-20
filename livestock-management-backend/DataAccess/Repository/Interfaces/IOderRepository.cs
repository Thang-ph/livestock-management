using BusinessObjects.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface IOderRepository
    {
        Task<ListOrders> GetListOrder(ListOrderFilter filter);
    }
}
