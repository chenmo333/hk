using MachineVision.Core;
using MachineVision.Defect.Models;
using MachineVision.Defect.Services;
using MachineVision.Shared.Services.Session;
using Prism.Services.Dialogs;

namespace MachineVision.Defect.ViewModels
{
    internal class CreateProjectViewModel : HostDialogViewModel
    {
        public CreateProjectViewModel(ProjectService appService)
        {
            this.appService = appService;
        }

        private string name;
        private readonly ProjectService appService;

        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(); }
        }

        public override async Task Save()
        {
            await appService.CreateOrUpdateAsync(new ProjectModel()
            {
                Name = Name,
            });
            await base.Save();
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
