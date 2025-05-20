using BusinessObjects.Dtos;
using BusinessObjects.Models;
using DataAccess.Repository.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface IMedicalRepository
    {
        Task<bool> MedicalExists(string id);

        Task<ListMedicine> GetListMedicineAsync(MedicinesFliter query);

        Task<MedicineSummary?> GetByIdAsync(string id);

        Task<MedicineDTO> CreateAsync(CreateMedicineDTO medicineModels);

        Task<MedicineSummary?> UpdateAsync(string id, UpdateMedicineDTO updateMedicineDto);

        Task<bool> DeleteAsync(string id);
        Task<List<MedicineDTO>> GetMedicineByDisease(string diseaseId);
    }
}
