using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using HalconDotNet;
using MachineVision.Core;
using MachineVision.Core.TemplateMatch;
using MachineVision.Shared.Controls;
using MvCameraControl;
using Prism.Commands;

namespace MachineVision.TemplateMatch.ViewModels
{
    
   public class HikViewModel : NavigationViewModel
    {
        public class Camera
        {
            public string CameraName { get; set; }
            public IDeviceInfo DeviceInfo { get; set; }
        }
        public ITemplateMatchService MatchService { get; set; }

        #region 相机标志位
        readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
    | DeviceTLayerType.MvGenTLGigEDevice | DeviceTLayerType.MvGenTLCXPDevice | DeviceTLayerType.MvGenTLCameraLinkDevice | DeviceTLayerType.MvGenTLXoFDevice;

        List<IDeviceInfo> deviceInfoList = new List<IDeviceInfo>();
        IDevice device = null;

        bool isGrabbing = false;        // ch:是否正在取图 | en: Grabbing flag

        //相机曝光
        double exposureTime;

        //相机增益
        double gain;



        #endregion


        private HObject image;
     public HikViewModel()
        {
            ScanCameraCommand = new DelegateCommand(ScanCamera);
            StartCameraCommand = new DelegateCommand(StartCamera);
            CaptureImageCommand = new DelegateCommand(CaptureImage);
            StopCameraCommand = new DelegateCommand(StopCamera);
            SaveImageCommand = new DelegateCommand(SaveImage);
        }
        
        public DelegateCommand ScanCameraCommand { get; private set; }
        public DelegateCommand StartCameraCommand { get; private set; }
        public DelegateCommand CaptureImageCommand { get; private set; }
        public DelegateCommand StopCameraCommand { get; private set; }
        public DelegateCommand SaveImageCommand { get; private set; }
        public ObservableCollection<Camera> CameraList { get; set; } = new ObservableCollection<Camera>();
        public Camera SelectedCamera { get; set; }  // 用于存储选中的相机


        private void ScanCamera()
        {
            SDKSystem.Initialize(); // 相机初始化SDK资源

            // 检查 CameraList 是否已初始化
            if (CameraList == null)
            {
                CameraList = new ObservableCollection<Camera>(); // 如果未初始化，进行初始化
            }

            // 清空 CameraList
            CameraList.Clear();

            // 枚举设备
            int nRet = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);

            // 在 CameraList 中添加设备
            foreach (var deviceInfo in deviceInfoList)
            {
                string cameraName = !string.IsNullOrEmpty(deviceInfo.UserDefinedName) ?
                    $"{deviceInfo.TLayerType}: {deviceInfo.UserDefinedName} ({deviceInfo.SerialNumber})" :
                    $"{deviceInfo.TLayerType}: {deviceInfo.ManufacturerName} {deviceInfo.ModelName} ({deviceInfo.SerialNumber})";

                CameraList.Add(new Camera
                {
                    CameraName = cameraName,
                    DeviceInfo = deviceInfo
                });
            }
        }


        private void StartCamera()
        {

        }
        private void CaptureImage()
        {

        }
        private void StopCamera()
        {

        }
        private void SaveImage()
        {

        }

        public HObject Image
        {
            get { return image; }
            set { image = value; RaisePropertyChanged(); }
        }
    }
}
