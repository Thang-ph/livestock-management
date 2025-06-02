using AutoMapper;
using BusinessObjects;
using BusinessObjects.ConfigModels;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Services
{
    public class DiseaseService : IDiseaseRepository
    {
        private readonly LmsContext _context;
        private readonly IMapper _mapper;

        public DiseaseService(LmsContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ListDiseases> GetListDiseases(string keyword)
        {
            var result = new ListDiseases()
            {
                Total = 0,
            };

            var listDiseases = await _context.Diseases
                .Where(o => string.IsNullOrEmpty(keyword) 
                    || o.Name.ToLower().Contains(keyword.Trim().ToLower()))
                .Select(o => new DiseaseDTO
                {
                    Id = o.Id,
                    Name = o.Name,
                    Symptom = o.Symptom,
                    Description = o.Description,
                    DefaultInsuranceDuration = o.DefaultInsuranceDuration ?? 21,
                    Type = o.Type,
                    CreatedAt = o.CreatedAt
                })
                .OrderByDescending(v => v.CreatedAt)
                .ToArrayAsync();
            if (listDiseases == null || !listDiseases.Any())
                return result;

            result.Total = listDiseases.Length;
            result.Items = listDiseases;

            return result;
        }

        public async Task<DiseaseDTO> UpdateDisease(string id, DiseaseUpdateDTO model)
        {
            var disease = await _context.Diseases.FirstOrDefaultAsync(x => x.Id == id.Trim());
            if (disease == null) throw new Exception("Không tìm thấy bệnh");
            disease.Name = model.Name;
            disease.Symptom = model.Symptom;
            disease.Description = model.Description;
            disease.DefaultInsuranceDuration = model.DefaultInsuranceDuration ?? 21;
            disease.Type = model.Type;
            disease.UpdatedAt = DateTime.Now;
            disease.UpdatedBy = model.requestedBy ?? "SYS";
            await _context.SaveChangesAsync();
            var result = new DiseaseDTO
            {
                Id = disease.Id,
                Name = disease.Name,
                Symptom = disease.Symptom,
                Description = disease.Description,
                DefaultInsuranceDuration = disease.DefaultInsuranceDuration ?? 21,
                Type = disease.Type,
                CreatedAt = disease.CreatedAt
            };
            return result;
        }

        public async Task<bool> DeleteDisease(string id)
        {
            var data = false;
            var disease = await _context.Diseases.FirstOrDefaultAsync(x => x.Id == id.Trim());
            if (disease == null) throw new Exception("Không tìm thấy bệnh");
            bool existsInDiseaseMedicines = await _context.DiseaseMedicines.AnyAsync(dm => dm.DiseaseId == id);
            bool existsInMedicalHistories = await _context.MedicalHistories.AnyAsync(mh => mh.DiseaseId == id);
            if (existsInDiseaseMedicines || existsInMedicalHistories)
                throw new Exception("Không thể xóa vì bệnh này đang được sử dụng trong hệ thống.");
            _context.Diseases.Remove(disease);
            await _context.SaveChangesAsync();
            data = true;
            return data;
        }

        public async Task<DiseaseDTO?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new Exception("Không có loại bệnh này trong hệ thống");
            var data = await _context.Diseases.FirstOrDefaultAsync(x => x.Id == id.Trim());
            if (data == null) throw new Exception("Không có loại bệnh này trong hệ thống");
            var result = _mapper.Map<DiseaseDTO>(data);
            return result;
        }

        public async Task<DiseaseDTO> CreateDisease(DiseaseUpdateDTO model)
        {
            bool isDuplicate = await _context.Diseases.AnyAsync(m => m.Name.ToLower().Trim() == model.Name.ToLower().Trim() && m.Type == model.Type);
            if (isDuplicate) throw new Exception("Tên loại bệnh này đã có trong hệ thống");
            var disease = new Disease();
            disease.Id = SlugId.New();
            disease.Name = model.Name;
            disease.Symptom = model.Symptom;
            disease.Description = model.Description;
            disease.DefaultInsuranceDuration = model.DefaultInsuranceDuration ?? 21;
            disease.Type = model.Type;
            disease.CreatedAt = DateTime.Now;
            disease.CreatedBy = model.requestedBy ?? "SYS";
            disease.UpdatedAt = DateTime.Now;
            disease.UpdatedBy = model.requestedBy ?? "SYS";
            await _context.Diseases.AddAsync(disease);
            await _context.SaveChangesAsync();
            var result = new DiseaseDTO
            {
                Id = disease.Id,
                Name = disease.Name,
                Symptom = disease.Symptom,
                Description = disease.Description,
                DefaultInsuranceDuration = disease.DefaultInsuranceDuration ?? 21,
                Type = disease.Type,
                CreatedAt = disease.CreatedAt
            };
            return result;
        }

        public async Task<StatsDiseaseSummary> GetStatsDiseaseByMonth(GetStatsDiseaseByMonthFilter filter)
        {
            var result = new StatsDiseaseSummary
            {
                Total = 0
            };

            DateTime defaultStartDate = DateTime.Now.AddMonths(-12);
            DateTime defaultEndDate = DateTime.Now;
            if (filter != null && filter.StartDate != null)
                defaultStartDate = (DateTime)filter.StartDate;
            if (filter != null && filter.EndDateDate != null)
                defaultEndDate = (DateTime)filter.EndDateDate;

            var medicalHistories = await _context.MedicalHistories
                .Where(o => o.CreatedAt >= defaultStartDate
                    && o.CreatedAt <= defaultEndDate
                    && (filter == null || string.IsNullOrEmpty(filter.DiseaseId) || o.DiseaseId == filter.DiseaseId)
                )
                .Select(o => new
                {
                    o.DiseaseId,
                    o.CreatedAt,
                    StringDate = o.CreatedAt.ToString("MMM-yy")
                })
                .ToArrayAsync();
            if (!medicalHistories.Any())
                return result;
            var totalQuantity = medicalHistories.Length;

            var diseaseIds = medicalHistories
                .GroupBy(o => o.DiseaseId)
                .Select(o => o.Key)
                .ToArray();
            var dicDiseases = await _context.Diseases
                .Where(o => diseaseIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.Name);
            if (!dicDiseases.Any())
                return result;

            var resultItems = new List<StatsDiseaseByMonth>();
            foreach(var diseaseId in diseaseIds)
            {
                var diseaseName = dicDiseases[diseaseId];
                var historiesByDisease = medicalHistories
                    .Where(o => o.DiseaseId == diseaseId)
                    .GroupBy(o => o.StringDate)
                    .Select(o => new QuantityByMonth
                    {
                        Date = o.FirstOrDefault().CreatedAt,
                        StringDate = o.Key,
                        Quantity = o.Count(),
                        Ratio = o.Count() / totalQuantity * 100
                    })
                    .ToArray();
                var statsByDisease = new StatsDiseaseByMonth
                {
                    DiseaseId = diseaseId,
                    DiseaseName = diseaseName,
                    quantitiesByMonth = historiesByDisease,
                };
                resultItems.Add(statsByDisease);
            }
            result.Items = resultItems
                .OrderBy(o => o.DiseaseName)
                .ToArray();
            result.Total = resultItems.Count();
            return result;  
        }

        public async Task<VaccinationRatioSummary> GetVaccinatedRatios()
        {
            var result = new VaccinationRatioSummary()
            {
                Total = 0,
                Items = new List<VaccinationRatio>()
            };

            var dicDiseases = await _context.Diseases
                .ToDictionaryAsync(o => o.Id, o => o.Name);
            if (!dicDiseases.Any())
                return result;

            var now = DateTime.Now;
            var dicMedicineDisease = await _context.DiseaseMedicines
               .ToDictionaryAsync(o => o.MedicineId, o => o.DiseaseId);
            var singleVaccinatedLivestocks = await _context.SingleVaccination
                .Where(o => EF.Functions.DateDiffDay(o.CreatedAt, now) < 21)
                .Select(o => new
                {
                    LivestockId = o.LivestockId,
                    MedicineId = o.MedicineId,
                    DiseaseId = dicMedicineDisease[o.MedicineId],
                    DiseaseName = dicDiseases[dicMedicineDisease[o.MedicineId]]
                })
                .ToListAsync();
            var livestockVaccination = await _context.LivestockVaccinations
                .Include(o => o.BatchVaccination)
                .Where(o => o.BatchVaccination.Status == batch_vaccination_status.HOÀN_THÀNH
                    && o.BatchVaccination.DateConduct != null
                    && EF.Functions.DateDiffDay((o.BatchVaccination.DateConduct ?? DateTime.MinValue), now) < 21)
                .Select(o => new
                {
                    LivestockId = o.LivestockId,
                    MedicineId = o.BatchVaccination.VaccineId,
                    DiseaseId = dicMedicineDisease[o.BatchVaccination.VaccineId],
                    DiseaseName = dicDiseases[dicMedicineDisease[o.BatchVaccination.VaccineId]]
                })
                .ToListAsync();
            var tmp = singleVaccinatedLivestocks.ToList();
            tmp.AddRange(livestockVaccination.ToList());
            var totalVaccinated = tmp.Distinct().ToArray();
            var totalQuantity = totalVaccinated.Length;

            var resultItems = totalVaccinated
                .GroupBy(o => new { o.DiseaseId, o.DiseaseName })
                .Select(o => new VaccinationRatio
                {
                    DiseaseId = o.Key.DiseaseId,
                    DiseaseName = o.Key.DiseaseName,
                    Ratio = o.Count() / totalQuantity * 100,
                    Severity = o.Count() / totalQuantity * 100 < 50 ?
                        severity.HIGH :
                        o.Count() / totalQuantity * 100 > 50 && o.Count() / totalQuantity * 100 < 70 ?
                        severity.MEDIUM : severity.LOW
                })
                .OrderBy(o => o.Ratio)
                .ToArray();
            result.Items = resultItems;
            result.Total = resultItems.Length;
            return result;
        }

        public async Task<DiseaseRatioSummary> GetDiseaseRatios()
        {
            var result = new DiseaseRatioSummary
            {
                Total = 0,
                Items = new List<DiseaseRatio>()
            };

            var medicalHistories = await _context.MedicalHistories
               .Where(o => o.Status == medical_history_status.CHỜ_KHÁM
                   || o.Status == medical_history_status.ĐANG_ĐIỀU_TRỊ
                   || o.Status == medical_history_status.TÁI_PHÁT)
               .ToArrayAsync();
            if (!medicalHistories.Any())
                return result;
            var totalQuantity = medicalHistories.Length;
            var diseaseIds = medicalHistories
                .GroupBy(o => o.DiseaseId)
                .Select(o => o.Key)
                .ToArray();
            var dicDiseases = await _context.Diseases
                .Where(o => diseaseIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, o => o.Name);
            var resultItems = medicalHistories
                .GroupBy(o => o.DiseaseId)
                .Select(o => new DiseaseRatio
                {
                    DiseaseId = o.Key,
                    DiseaseName = dicDiseases.ContainsKey(o.Key) ? dicDiseases[o.Key] : "N/A",
                    Quantity = o.Count(),
                    Ratio = o.Count() / totalQuantity * 100,
                    Severity = o.Count() / totalQuantity * 100 > 20 ?
                        severity.HIGH :
                        o.Count() / totalQuantity * 100 < 20 && o.Count() / totalQuantity * 100 > 10 ?
                        severity.MEDIUM : severity.LOW
                })
                .ToArray();
            result.Items = resultItems;
            result.Total = resultItems.Length;
            return result;
        }

        public async Task<int> GetCurrentDiseaseQuantity()
        {
            var medicalHistories = await _context.MedicalHistories
                .Where(o => o.Status == medical_history_status.CHỜ_KHÁM
                    || o.Status == medical_history_status.ĐANG_ĐIỀU_TRỊ
                    || o.Status == medical_history_status.TÁI_PHÁT)
                .GroupBy(o => o.DiseaseId)
                .Select(o => o.Key)
                .ToArrayAsync();
            return medicalHistories.Length;
        }
    }
}
