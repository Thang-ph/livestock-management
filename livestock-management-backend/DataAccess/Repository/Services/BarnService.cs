using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Math;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository.Services
{
    public class BarnService : IBarnRepository
    {
        private readonly LmsContext _context;
        public BarnService(LmsContext context)
        {
            _context = context;
        }

        public async Task<BarnInfo> GetBarnById(string id)
        {
            var data = await _context.Barns.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null) 
                throw new Exception("Không có trại này trong hệ thống");
            return new BarnInfo
            {
                Id = data.Id,
                Name = data.Name,
                Address = data.Address
            };
        }

        public async Task<ListBarns> GetListBarns()
        {
            var result = new ListBarns()
            {
                Total = 0
            };

            var listBarns = await _context.Barns
                .Select(o => new BarnInfo
                {
                    Id = o.Id,
                    Name = o.Name,
                    Address = o.Address,
                })
                .ToArrayAsync(); 
            if (listBarns == null || !listBarns.Any()) 
                return result;

            result.Items = listBarns;
            result.Total = listBarns.Length;

            return result;
        }

        public async Task<BarnInfo> CreateBarn(CreateBarnDTO createModel)
        {
            var data = await _context.Barns.FirstOrDefaultAsync(x => x.Name.ToLower() == createModel.Name.Trim().ToLower());
            if (data != null)
                throw new Exception("Trang trại này đã tồn tại trong hệ thống");
            var barn = new Barn
            {
                Id = SlugId.New(),
                Name = createModel.Name.Trim(),
                Address = createModel.Address.Trim(),
                CreatedAt = DateTime.Now,
                CreatedBy = createModel.RequestedBy.Trim() ?? "SYS",
                UpdatedAt = DateTime.Now,
                UpdatedBy = createModel.RequestedBy.Trim() ?? "SYS",
            };
            await _context.Barns.AddAsync(barn);
            await _context.SaveChangesAsync();
            return new BarnInfo 
            {
                Id = barn.Id,
                Name = barn.Name,
                Address = barn.Address
            };
        }


        public async Task<BarnInfo> UpdateBarn(string id, UpdateBarnDTO updateModel)
        {
            var dataExist = await _context.Barns.FirstOrDefaultAsync(x => x.Id == id);
            if (dataExist == null)
                throw new Exception("Trang trại này không tồn tại trong hệ thống");
            dataExist.Name = updateModel.Name.Trim();
            dataExist.Address = updateModel.Address.Trim();
            dataExist.UpdatedBy = updateModel.RequestedBy.Trim() ?? "SYS";
            dataExist.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return new BarnInfo
            {
                Id = dataExist.Id,
                Name = dataExist.Name,
                Address = dataExist.Address
            };
        }

        public async Task<bool> DeleteBarn(string id)
        {
            var dataExist = await _context.Barns.FirstOrDefaultAsync(x => x.Id == id);
            if (dataExist == null)
                throw new Exception("Trang trại này không tồn tại trong hệ thống");
            var batchExport = await _context.BatchExports.Where(x => x.BarnId == id.Trim()).ToArrayAsync();
            var liveStock = await _context.Livestocks.Where(x => x.BarnId == id.Trim()).ToArrayAsync();
            var batchImport = await _context.BatchImports.Where(x => x.BarnId == id.Trim()).ToArrayAsync();
            if (batchExport.Any() || liveStock.Any() || batchImport.Any())
                throw new Exception("Trang trại đang được sử dụng trong hệ thống, không thể xóa.");
            _context.Barns.Remove(dataExist);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
