using MachineVision.Core;
using MachineVision.Defect.Extensions;
using MachineVision.Defect.Models;
using MachineVision.Defect.ViewModels.Components;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MachineVision.Defect.ViewModels
{
    internal class TrainViewModel : DialogViewModel
    {
        public TrainViewModel()
        {
            Files = new ObservableCollection<ImageInfo>();
            DeleteCommand = new DelegateCommand(Delete);
        }

        #region 字段/属性

        private InspecRegionModel selectedRegion;

        public InspecRegionModel SelectedRegion
        {
            get { return selectedRegion; }
            set
            {
                selectedRegion = value;
                GetRegionIamges();
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<InspecRegionModel> regionList;

        public ObservableCollection<InspecRegionModel> RegionList
        {
            get { return regionList; }
            set { regionList = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ImageInfo> files;

        public ObservableCollection<ImageInfo> Files
        {
            get { return files; }
            set { files = value; RaisePropertyChanged(); }
        }

        private ImageInfo selectedFile;

        public ImageInfo SelectedFile
        {
            get { return selectedFile; }
            set
            {
                selectedFile = value;
                ShowImage(value);
                RaisePropertyChanged();
            }
        }

        private BitmapImage image;

        public BitmapImage Image
        {
            get { return image; }
            set { image = value; RaisePropertyChanged(); }
        }

        #endregion

        public DelegateCommand DeleteCommand { get; private set; }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Value"))
            {
                RegionList = parameters.GetValue<ObservableCollection<InspecRegionModel>>("Value");
            }

            base.OnDialogOpened(parameters);
        }

        /// <summary>
        /// 获取选中区域的训练图像集合
        /// </summary>
        private void GetRegionIamges()
        {
            var trainUrl = SelectedRegion.GetRegionTrainUrl();

            if (Directory.Exists(trainUrl))
            {
                var files = Directory.GetFiles(trainUrl);

                Files.Clear();
                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file);

                    if (ext != ".bmp") continue;

                    Files.Add(new ImageInfo()
                    {
                        FileName = Path.GetFileName(file),
                        FullPath = file,
                    });
                }
            }
        }

        /// <summary>
        /// 展示图像
        /// </summary>
        /// <param name="img"></param>
        private void ShowImage(ImageInfo img)
        {
            if (img == null) return;

            var bytes = File.ReadAllBytes(img.FullPath);
            MemoryStream ms = new MemoryStream(bytes);
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.StreamSource = ms;
            b.EndInit();
            Image = b;
        }

        /// <summary>
        /// 移除图像
        /// </summary>
        private void Delete()
        {
            if (SelectedRegion == null) return;
            if (SelectedFile == null) return;

            if (File.Exists(SelectedFile.FullPath))
            {
                //移除本地和界面上的数据
                File.Delete(SelectedFile.FullPath);
                var file = Files.FirstOrDefault(t => t.FileName == SelectedFile.FileName);
                if (file != null)
                    Files.Remove(file);

                if (SelectedRegion.Context is LocalDeformableContext context)
                    context.RefreshVariationModel(SelectedRegion);
            }
        }
    }
}
