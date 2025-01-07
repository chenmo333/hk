using AutoMapper;
using MachineVision.Defect.Models; 
using MachineVision.Shared.Services.Tables;

namespace MachineVision.Defect
{
    public class DefectProfile : Profile
    {
        public DefectProfile()
        {
            CreateMap<Project, ProjectModel>().ReverseMap();
            CreateMap<InspecRegion, InspecRegionModel>().ReverseMap();
        }
    }
}
