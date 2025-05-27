using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using BusinessObjects;
using DataAccess.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BusinessObjects.Constants;
using static BusinessObjects.Constants.LmsConstants;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DataAccess.Repository.Services
{
    public class SpecieService : ISpecieRepository
    {
        private readonly LmsContext _context;
        private readonly IMapper _mapper;

        public SpecieService(LmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SpecieDTO>> GetAllAsync()
        {
            var data = await _context.Species.ToListAsync();
            var result = _mapper.Map<List<SpecieDTO>>(data);
            return result;
        }

        public async Task<SpecieDTO?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new Exception("Không có loài vật này trong hệ thống");
            var data = await _context.Species.FirstOrDefaultAsync(x => x.Id == id.Trim());
            if (data == null) throw new Exception("Không có loài vật này trong hệ thống");
            var result = _mapper.Map<SpecieDTO>(data);
            return result;
        }

        public async Task<SpecieDTO> CreateAsync(SpecieCreate specieCreate)
        {
            bool isDuplicate = await _context.Species.AnyAsync(m => m.Name.ToLower() == specieCreate.Name.ToLower() && m.Type == specieCreate.Type);
            if (isDuplicate) throw new Exception("Tên loài vật này đã có trong hệ thống");
            var species = _mapper.Map<Species>(specieCreate);
            species.Id = SlugId.New();
            species.Name = specieCreate.Name;
            species.Description = specieCreate.Description;
            species.GrowthRate = specieCreate.GrowthRate;
            species.DressingPercentage = specieCreate.DressingPercentage;
            species.Type = specieCreate.Type;
            species.CreatedAt = DateTime.Now;
            species.CreatedBy = specieCreate.RequestedBy ?? "SYS";
            species.UpdatedAt = species.CreatedAt;
            species.UpdatedBy = species.CreatedBy;
            await _context.Species.AddAsync(species);
            await _context.SaveChangesAsync();
            var result = _mapper.Map<SpecieDTO>(species);
            return result;
        }

        public async Task<SpecieDTO?> UpdateAsync(string id, SpecieUpdate specieUpdate)
        {
            var species = await _context.Species.FirstOrDefaultAsync(x => x.Id == id.Trim());
            if (species == null) throw new Exception("Không có loài vật này trong hệ thống");
            // Check for duplicate name (excluding current species)
            var nameCheck = await _context.Species.Where(x => x.Name.ToLower().Trim() == specieUpdate.Name.ToLower().Trim()
                                        && x.Type == specieUpdate.Type
                                        && x.Id != id.Trim())
                                        .ToArrayAsync();
            if (nameCheck.Any())
                throw new Exception("Tên loài vật đã được sử dụng");
            species.Name = specieUpdate.Name;
            species.Description = specieUpdate.Description;
            species.GrowthRate = specieUpdate.GrowthRate;
            species.DressingPercentage = specieUpdate.DressingPercentage;
            species.Type = specieUpdate.Type;
            species.UpdatedAt = DateTime.Now;
            species.UpdatedBy = specieUpdate.RequestedBy ?? "SYS";
            await _context.SaveChangesAsync();
            var result = _mapper.Map<SpecieDTO>(species);
            return result;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var specie = await _context.Species.FirstOrDefaultAsync(x => x.Id == id.Trim());
            if (specie == null) throw new Exception("Không có loài vật này trong hệ thống");
            var procurementDetails = await _context.ProcurementDetails.Where(x => x.SpeciesId == id.Trim()).ToArrayAsync();
            var liveStock = await _context.Livestocks.Where(x => x.SpeciesId == id.Trim()).ToArrayAsync();
            if (procurementDetails.Any() || liveStock.Any())
                throw new Exception("Loài vật này đang được sử dụng trong hệ thống, không thể xóa.");
            _context.Species.Remove(specie);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SpecieDTO>> GetListCanDeleteSpecies()
        {
            var species = await _context.Species
                .Where(s => !_context.ProcurementDetails.Any(a => a.SpeciesId == s.Id))
                 .Where(s => !_context.Livestocks.Any(a => a.SpeciesId == s.Id))
                .ToListAsync();
            List<SpecieDTO> result = new List<SpecieDTO>();
            _mapper.Map(species, result);
            return result;
        }

        public async Task<List<SpecieName>> GetListSpecieNameByType(specie_type type)
        {
            var specieName = await _context.Species
           .Where(x => x.Type == type)
           .Select(s => new SpecieName
           {
               Id = s.Id,
               Name = s.Name
           })
           .ToListAsync();

            if (!specieName.Any())
                throw new Exception("Không tồn tại loài vật này");
            return specieName;
        }
    }
}
