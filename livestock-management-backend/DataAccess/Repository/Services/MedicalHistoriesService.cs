using BusinessObjects;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Services
{
    public class MedicalHistoriesService : IMedicalHistoriesRepository
    {
        private readonly LmsContext _context;

        public MedicalHistoriesService(LmsContext context)
        {
            _context = context;
        }
        public async Task<List<MedicalHistoriesGeneral>> GetMedicalHistoriesGeneralInfoById(string livestockId)
        {
            var livestock = await _context.Livestocks.FirstOrDefaultAsync(x => x.Id == livestockId);
            if (livestock == null)
            {
                throw new Exception("Không tìm thấy vật nuôi");
            }

            var medicalHistories = await _context.MedicalHistories
                .Include(x => x.Medicine)
                .Include(x => x.Disease)
                .Where(x => x.LivestockId == livestockId)
                .ToListAsync();

            if (!medicalHistories.Any()) 
            {
                return new List<MedicalHistoriesGeneral>();
            }

            var medicalHistoriesSummary = medicalHistories.Select(x => new MedicalHistoriesGeneral
            {
                CreatedAt = x.CreatedAt,
                DiseaseName = x.Disease?.Name, 
                MedicineName = x.Medicine?.Name 
            }).ToList();

            return medicalHistoriesSummary;
        }

    }
}
