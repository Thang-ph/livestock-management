using BusinessObjects.Dtos;
using BusinessObjects.Models;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using BusinessObjects.ConfigModels;
using DataAccess.Repository.Interfaces;
using AutoMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Repository.Services
{
    public class MedicalService : IMedicalRepository
    {
        private readonly LmsContext _context;
        private readonly IMapper _mapper;

        public MedicalService(LmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> MedicalExists(string id)
        {
            return await _context.Medicines.AnyAsync(x => x.Id == id.Trim());
        }

        public async Task<ListMedicine> GetListMedicineAsync(MedicinesFliter filter)
        {
            var result = new ListMedicine()
            {
                Total = 0
            };

            var medicine = await _context.Medicines.ToArrayAsync();
            if (medicine == null || !medicine.Any()) return result;

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Keyword))
                {
                    medicine = medicine
                        .Where(v => v.Name.ToUpper().Contains(filter.Keyword.Trim().ToUpper()))
                        .ToArray();
                }
                if (filter.FromDate != null && filter.FromDate != DateTime.MinValue)
                {
                    medicine = medicine
                        .Where(v => v.CreatedAt >= filter.FromDate)
                        .ToArray();
                }
                if (filter.ToDate != null && filter.ToDate != DateTime.MinValue)
                {
                    medicine = medicine
                       .Where(v => v.CreatedAt <= filter.ToDate)
                       .ToArray();
                }
                if (filter.Type != null)
                {
                    medicine = medicine
                        .Where(v => v.Type == filter.Type)
                        .ToArray();
                }
            }
            if (!medicine.Any()) return result;
            medicine = medicine
                .OrderByDescending(v => v.CreatedAt)
                .ToArray();

            result.Items = medicine
                .Select(v => new MedicineSummary
                {
                    Id = v.Id,
                    Name = v.Name,
                    Description = v.Description,
                    Type = v.Type,
                    CreatedAt = v.CreatedAt,
                })
                .OrderByDescending(v => v.CreatedAt)
                .Skip(filter?.Skip ?? 0)
                .Take(filter?.Take ?? 10)
                .ToArray();
            result.Total = medicine.Length;

            return result;
        }

        public async Task<MedicineSummary?> GetByIdAsync(string id)
        {
            var medicineModels = await _context.Medicines.FirstOrDefaultAsync( x => x.Id == id.Trim());
            if (medicineModels == null) throw new Exception("Không tìm thấy thuốc.");
            var medicineViews = _mapper.Map<MedicineSummary>(medicineModels);
            return medicineViews;
        }

        public async Task<MedicineDTO> CreateAsync(CreateMedicineDTO medicine)
        {
            bool isDuplicate = await _context.Medicines.AnyAsync(m => m.Name.ToLower() == medicine.Name.ToLower());
            if (isDuplicate) throw new Exception("Tên thuốc trùng");
            var medicineModels = _mapper.Map<Medicine>(medicine);
            medicineModels.Id = SlugId.New();
            medicineModels.Name = medicineModels.Name.Trim();
            medicineModels.Description = medicineModels.Description.Trim();
            medicineModels.Type = medicineModels.Type;
            medicineModels.CreatedAt = DateTime.Now;
            medicineModels.CreatedBy = medicineModels.CreatedBy.Trim();
            medicineModels.UpdatedAt = medicineModels.CreatedAt;
            medicineModels.UpdatedBy = medicineModels.CreatedBy;
            await _context.Medicines.AddAsync(medicineModels);
            await _context.SaveChangesAsync();
            var medicineView = _mapper.Map<MedicineDTO>(medicineModels);
            return medicineView;
        }

        public async Task<MedicineSummary?> UpdateAsync(string id, UpdateMedicineDTO updateMedicineDto)
        {
            var medicineModels = await _context.Medicines.FirstOrDefaultAsync(x => x.Id == id.Trim());
            if (medicineModels == null) throw new Exception("Không tìm thấy thuốc.");
            medicineModels.Name = updateMedicineDto.Name.Trim();
            medicineModels.Description = updateMedicineDto.Description.Trim();
            medicineModels.Type = updateMedicineDto.Type;
            medicineModels.UpdatedAt = DateTime.Now;
            medicineModels.UpdatedBy = updateMedicineDto.UpdatedBy.Trim();
            await _context.SaveChangesAsync();
            var medicineView = _mapper.Map<MedicineSummary>(medicineModels);
            return medicineView;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var medicineModels = await _context.Medicines.FirstOrDefaultAsync( x => x.Id == id.Trim());
            if (medicineModels == null) throw new Exception("Không tìm thấy thuốc.");
            var diseaseMedicine = await _context.DiseaseMedicines.Where(x => x.MedicineId == id.Trim()).ToArrayAsync();
            var medicalHistories = await _context.MedicalHistories.Where(x => x.Id == id.Trim()).ToArrayAsync();
            var batchVaccination = await _context.BatchVaccinations.Where(x => x.VaccineId == id.Trim()).ToArrayAsync();
            if(diseaseMedicine.Any() || medicalHistories.Any() || batchVaccination.Any()) 
                throw new Exception("Thuốc đang được sử dụng trong hệ thống, không thể xóa.");
            _context.Medicines.Remove(medicineModels);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MedicineDTO>> GetMedicineByDisease(string diseaseId)
        {
            List<MedicineDTO> medicineDTOs = new List<MedicineDTO>();
            List<Medicine> listMedicine = _context.Medicines
      .Where(m => m.DiseaseMedicines.Any(h => h.Disease.Id == diseaseId))
      .ToList();
            foreach (Medicine medicine in listMedicine)
            {
                var medicineDTO = new MedicineDTO
                {
                    Id = medicine.Id,
                    Name = medicine.Name,
                    Description = medicine.Description,
                    Type = medicine.Type,
                    CreatedAt = medicine.CreatedAt,
                    CreatedBy = medicine.CreatedBy,
                    UpdatedAt = medicine.UpdatedAt,
                    UpdatedBy = medicine.UpdatedBy
                };
                medicineDTOs.Add(medicineDTO);
            }
            return medicineDTOs;
        }
    }
}
