using AutoMapper;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Presentation;
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
    public class CustomerService : ICustomerRepository
    {
        private readonly LmsContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;

        public CustomerService(LmsContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _mapper = mapper;
            _context = context;
        }
        public async Task<CustomerInfo> AddCustomer(AddCustomerDTO model)
        {
            var customer = await _context.Customers
                .Where(x => x.Fullname.ToLower().Trim() == model.CustomerName.ToLower().Trim()
                && x.Phone == model.Phone)
                .ToArrayAsync();
            if (customer.Any())
                throw new Exception("Nguời dùng đã tồn tại trong hệ thống");

            var customerData = new Customer();
            customerData.Id = SlugId.New();
            customerData.Fullname = model.CustomerName;
            customerData.Phone = model.Phone;
            customerData.Address = model.Address ?? null;
            customerData.Email = model.Email ?? null;
            customerData.CreatedAt = DateTime.Now;
            customerData.CreatedBy = model.RequestedBy ?? "SYS";
            customerData.UpdatedAt = DateTime.Now;
            customerData.UpdatedBy = model.RequestedBy ?? "SYS";

            await _context.Customers.AddAsync(customerData);
            await _context.SaveChangesAsync();

            return new CustomerInfo
            {
                Id = customerData.Id,
                CustomerName = customerData.Fullname,
                Phone = customerData.Phone,
                Address = customerData.Address ?? null,
                Email = customerData.Email ?? null,
            };
        }

        public async Task<CustomerInfo> GetCustomerInfo(string customerId)
        {
           var customerData = await _context.Customers
                .FirstOrDefaultAsync(x => x.Id == customerId);
            if (customerData == null)
                throw new Exception("Không tìm thấy người dùng trong hệ thống");

            return new CustomerInfo
            {
                Id = customerData.Id,
                CustomerName = customerData.Fullname,
                Phone = customerData.Phone,
                Address = customerData.Address ?? null,
                Email = customerData.Email ?? null,
                CreatedAt = customerData.CreatedAt,
            };
        }
    }
}
