using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            Template1();
         
        }
        public async Task Template1()
        {
            await Task.Run(() =>
            {
                // 获取选择的设备信息 | Get selected device information
                IDeviceInfo deviceInfo = SelectedCamera?.DeviceInfo; // 使用绑定的 SelectedCamera 获取 DeviceInfo

                try
                {
                    // 创建设备
                    device = DeviceFactory.CreateDevice(deviceInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("创建设备失败！" + ex.Message);
                    return;
                }

                int result = device.Open();


                // 判断是否为GigE设备并设置包大小
                if (device is IGigEDevice gigEDevice)
                {
                    int optionPacketSize;
                    result = gigEDevice.GetOptimalPacketSize(out optionPacketSize);
                    if (result == MvError.MV_OK)
                    {
                        result = device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optionPacketSize);

                    }

                }

                // 设置心跳超时
                device.Parameters.SetIntValue("GevHeartbeatTimeout", 3000);

                // **设置触发源为软件**
                result = device.Parameters.SetEnumValueByString("TriggerSource", "Software");


                // **启用触发模式**
                result = device.Parameters.SetEnumValueByString("TriggerMode", "On");


                // 设置采集模式为连续
                result = device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");


                StartGrab();
            });
        }

        private void StartGrab()
        {
            Template2();
        }
        public async Task Template2()
        {
            await Task.Run(() =>
            {

                isGrabbing = true;

                // ch:开始采集 | en:Start Grabbing
                int result = device.StreamGrabber.StartGrabbing();
                if (result != MvError.MV_OK)
                {
                    isGrabbing = false;

                    return;
                }
            });
        }
        private void CaptureImage()
        {

            int nRet;
            IFrameOut frameOut;
            HImage hImage = new HImage();//读取主图片
            nRet = device.StreamGrabber.GetImageBuffer(500, out frameOut);
            if (nRet != MvError.MV_OK)
            {
                return;
            }
            else
            {
                var img = new HImage();
                Image mimage = frameOut.Image.ToBitmap();

                // 指定保存路径
                string filePath = @"C:\image.jpg"; // 可以修改文件名和格式

                // 将图像保存到指定路径
                mimage.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg); // 可以根据需要修改格式，如 PNG、BMP 等

                hImage.ReadImage("C:/image.jpg");
                Image = hImage;

                device.StreamGrabber.FreeImageBuffer(frameOut);
            }

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
