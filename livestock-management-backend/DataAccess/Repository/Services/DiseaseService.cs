using AutoMapper;
using BusinessObjects;
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
                .Select(o => new DiseaseSummary
                {
                    Id = o.Id,
                    Name = o.Name
                })
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
            disease.Type = model.Type;
            disease.UpdatedAt = DateTime.Now;
            disease.UpdatedBy = model.requestedBy.IsNullOrEmpty() ? "SYS" : model.requestedBy;
            await _context.SaveChangesAsync();
            var diseaseViews = _mapper.Map<DiseaseDTO>(disease);
            return diseaseViews;
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
    }
}
