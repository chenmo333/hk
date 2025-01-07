using MachineVision.Core;
using MachineVision.Defect.Models;
using MachineVision.Defect.Services;
using MachineVision.Defect.ViewModels.Components;
using MachineVision.Defect.ViewModels.Components.Models;
using MachineVision.Shared.Services.Session;
using Prism.Commands;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;

namespace MachineVision.Defect.ViewModels
{
    internal class RegionParameterViewModel : HostDialogViewModel
    {
        public RegionParameterViewModel(ProjectService appService)
        {
            AddParameterCommand = new DelegateCommand(AddParameter);
            DeleteParameterCommand = new DelegateCommand<VariationParameter>(DeleteParameter);
            this.appService = appService;
        }

        public DelegateCommand AddParameterCommand { get; private set; }
        public DelegateCommand<VariationParameter> DeleteParameterCommand { get; private set; }

        private InspecRegionModel model;
        private readonly ProjectService appService;

        public InspecRegionModel Model
        {
            get { return model; }
            set { model = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// 添加检测参数
        /// </summary>
        private void AddParameter()
        {
            if (Model.Context != null && Model.Context is LocalDeformableContext context)
            {
                var param = new VariationParameter();
                param.ApplyDefaultValue();

                if (context.Setting.Parameters == null)
                    context.Setting.Parameters = new ObservableCollection<VariationParameter>();

                context.Setting.Parameters.Add(param);
            }
        }

        /// <summary>
        /// 删除检测参数
        /// </summary>
        /// <param name="parameter"></param>
        private void DeleteParameter(VariationParameter parameter)
        {
            if (Model.Context != null && Model.Context is LocalDeformableContext context)
            {
                var param = context.Setting.Parameters.FirstOrDefault(t => t.Equals(parameter));
                if (param != null)
                {
                    context.Setting.Parameters.Remove(param);
                }
            }
        }

        public override async Task Save()
        {
            if (Model.Context is LocalDeformableContext context)
            {
                context.Setting.InitParameters();
            }

            await appService.UpdateRegionAsync(Model);
            await base.Save();
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Value"))
                Model = parameters.GetValue<InspecRegionModel>("Value");
        }
    }
}
