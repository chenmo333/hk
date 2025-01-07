using MachineVision.Defect.Services;
using MachineVision.Defect.ViewModels;
using MachineVision.Defect.Views; 
using Prism.Ioc;
using Prism.Modularity;

namespace MachineVision.Defect
{
    public class DefectModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry services)
        {
            services.Register<TargetService>();
            services.Register<ProjectService>();
            services.RegisterSingleton<InspectionService>();

            services.RegisterForNavigation<CreateProjectView, CreateProjectViewModel>();
            services.RegisterForNavigation<DefectView, DefectViewModel>();
            services.RegisterForNavigation<DefectEditView, DefectEditViewModel>();

            services.RegisterForNavigation<RegionParameterView, RegionParameterViewModel>();
            services.RegisterDialog<TrainView, TrainViewModel>();
        }
    }
}
