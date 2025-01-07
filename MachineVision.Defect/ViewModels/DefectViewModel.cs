using MachineVision.Core;
using MachineVision.Defect.Models;
using MachineVision.Defect.Services;
using MachineVision.Shared.Services.Session;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;

namespace MachineVision.Defect.ViewModels
{
    internal class DefectViewModel : NavigationViewModel
    {
        private readonly ProjectService appService;
        private readonly IRegionManager region;
        private readonly IHostDialogService dialog;

        public DefectViewModel(ProjectService appService,
            IRegionManager region,
            IHostDialogService dialog)
        {
            FilterText = string.Empty;
            Models = new ObservableCollection<ProjectModel>();
            this.appService = appService;
            this.region = region;
            this.dialog = dialog;
            CreateCommand = new DelegateCommand(Create);
            SearchCommand = new DelegateCommand(Search);
            DeleteCommand = new DelegateCommand<ProjectModel>(Delete);
            EditCommand = new DelegateCommand<ProjectModel>(Edit);
        }

        private string filterText;

        public string FilterText
        {
            get { return filterText; }
            set
            {
                filterText = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<ProjectModel> models;

        public ObservableCollection<ProjectModel> Models
        {
            get { return models; }
            set { models = value; RaisePropertyChanged(); }
        }

        public DelegateCommand CreateCommand { get; private set; }
        public DelegateCommand SearchCommand { get; private set; }
        public DelegateCommand<ProjectModel> DeleteCommand { get; private set; }
        public DelegateCommand<ProjectModel> EditCommand { get; private set; }

        /// <summary>
        /// 选择项目
        /// </summary>
        /// <param name="model"></param>
        private void Edit(ProjectModel model)
        {
            model.InitParameter();

            NavigationParameters param = new NavigationParameters();
            param.Add("Value", model);

            region.Regions["MainViewRegion"].RequestNavigate("DefectEditView", back =>
            {
                if (!(bool)back.Result)
                {
                    System.Diagnostics.Debug.WriteLine(back.Error.Message);
                }
            }, param);
        }

        private async void Delete(ProjectModel model)
        {
            if (model == null) return;

            await appService.DeleteAsync(model.Id);
            await GetListAsync();
        }

        private async void Search()
        {
            await GetListAsync();
        }

        private async void Create()
        {
            var dialogResult = await dialog.ShowDialogAsync("CreateProjectView");
            if (dialogResult.Result == ButtonResult.OK)
            {
                await GetListAsync();
            }
        }

        public async Task GetListAsync()
        {
            var list = await appService.GetListAsync(FilterText);

            Models.Clear();
            foreach (var item in list)
                Models.Add(item);
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            await GetListAsync();
            base.OnNavigatedTo(navigationContext);
        }
    }
}
