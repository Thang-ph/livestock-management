using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BusinessObjects.Constants.LmsConstants;

namespace DataAccess.AutoMapperConfig
{
    public class ConfigMapper : Profile
    {
        public ConfigMapper() 
        {
            //Medical
            CreateMap<Medicine, MedicineDTO>().ReverseMap();
            CreateMap<Medicine, MedicineSummary>().ReverseMap();
            CreateMap<Medicine, CreateMedicineDTO>().ReverseMap();
            CreateMap<Medicine, UpdateMedicineDTO>().ReverseMap();

            //ProcurementPackage
            CreateMap<ProcurementPackage, ProcurementPackageDto>().ReverseMap();
            CreateMap<ProcurementPackage, CreateProcurementPackageDTO>().ReverseMap();
            CreateMap<ProcurementPackage, UpdateMedicineDTO>().ReverseMap();

            //Specie
            CreateMap<Species, SpecieDTO>().ReverseMap();
            CreateMap<Species, SpecieCreate>().ReverseMap();
            CreateMap<Species, SpecieUpdate>().ReverseMap();
            //BatchExports
            CreateMap<BatchExport, BatchExportDTO>()
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.CustomerPhone))
                .ReverseMap();

            //BatchImports
            CreateMap<BatchImport, ImportBatchSummary>().ReverseMap();
            CreateMap<BatchImport, BatchImportGet>().ReverseMap();

            //BatchVacination
            CreateMap<BatchVaccination, BatchVacinationCreate>().ReverseMap();

            //Disease
            CreateMap<Disease, DiseaseDTO>().ReverseMap();

            //InspectionCodeRange
            CreateMap<InspectionCodeRange, InspectionCodeRangeDTO>()
                .ForMember(dest => dest.SpecieTypeList, opt => opt.MapFrom(src => JsonConvert.DeserializeObject<List<String>>(src.SpecieTypes))) // Convert từ chuỗi JSON thành List<specie_type>
                                                                                                                                                      // Convert từ List<specie_type> thành chuỗi JSON
                .ReverseMap();

            CreateMap<CreateInspectionCodeRangeDTO, InspectionCodeRange>()
                .ForMember(dest => dest.SpecieTypes, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.SpecieTypeList))) // Convert từ chuỗi JSON thành List<specie_type>
                .ForMember(dest => dest.StartCode, opt => opt.MapFrom(src => src.StartCode))
                .ForMember(dest => dest.EndCode, opt => opt.MapFrom(src => src.EndCode))
                .ForMember(dest => dest.CurrentCode, opt => opt.MapFrom(src => src.StartCode));


            CreateMap<InsuranceRequest, InsurenceRequestDTO>().ReverseMap();
        }
    }
}
