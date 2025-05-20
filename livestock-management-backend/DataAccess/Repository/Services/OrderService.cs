using AutoMapper;
using BusinessObjects;
using BusinessObjects.Dtos;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Services
{
    public class OrderService : IOderRepository
    {
        private readonly LmsContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public OrderService(LmsContext context, IMapper mapper,UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _context = context;
        }

        public async Task<ListOrders> GetListOrder(ListOrderFilter filter)
        {
            var result = new ListOrders
            {
                Total = 0,
            };

            var order = await _context.Orders
                .Include(c => c.Customer)
                .ToArrayAsync();
            if(order == null || !order.Any())
                return result;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    order = order
                        .Where(v => v.Customer.Fullname.ToUpper().Contains(filter.Keyword.Trim().ToUpper()))
                        .ToArray();
                }
                if (filter.FromDate != null && filter.FromDate != DateTime.MinValue)
                {
                    order = order
                        .Where(v => v.CreatedAt >= filter.FromDate)
                        .ToArray();
                }
                if (filter.ToDate != null && filter.ToDate != DateTime.MinValue)
                {
                    order = order
                       .Where(v => v.CreatedAt <= filter.ToDate)
                       .ToArray();
                }
                if (filter.Status != null)
                {
                    order = order
                        .Where(v => v.Status == filter.Status)
                        .ToArray();
                }
            }
            if (!order.Any()) return result;
            order = order
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();

            result.Items = order
                .Select(v => new OrderSummary
                {
                    Id = v.Id,
                    CustomerName = v.Customer.Fullname,
                    PhoneNumber = v.Customer.Phone,
                    Total = 1,
                    Received = 1,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .OrderByDescending(v => v.CreatedAt)
                .Skip(filter?.Skip ?? 0)
                .Take(filter?.Take ?? 10)
                .ToArray();
            result.Total = order.Length;

            return result;
        }


    }
}
