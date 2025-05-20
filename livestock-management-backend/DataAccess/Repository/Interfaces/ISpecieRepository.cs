using BusinessObjects.Constants;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.Repository.Interfaces
{
    public interface ISpecieRepository
    {
        Task<List<SpecieDTO>> GetAllAsync();
        Task<SpecieDTO?> GetByIdAsync(string id);
        Task<SpecieDTO> CreateAsync(SpecieCreate specieCreate);
        Task<SpecieDTO?> UpdateAsync(string id, SpecieUpdate specieUpdate);
        Task<bool> DeleteAsync(string id);
        Task<List<SpecieDTO>> GetListCanDeleteSpecies();
        Task<List<SpecieName>> GetListSpecieNameByType(specie_type type);
    }
}
